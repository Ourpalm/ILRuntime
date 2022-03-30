/*---------------------------------------------------------
 * Copyright (C) Microsoft Corporation. All rights reserved.
 *--------------------------------------------------------*/
/*
 * extension.ts (and activateMockDebug.ts) forms the "plugin" that plugs into VS Code and contains the code that
 * connects VS Code with the debug adapter.
 * 
 * extension.ts contains code for launching the debug adapter in three different ways:
 * - as an external program communicating with VS Code via stdin/stdout,
 * - as a server process communicating with VS Code via sockets or named pipes, or
 * - as inlined code running in the extension itself (default).
 * 
 * Since the code in extension.ts uses node.js APIs it cannot run in the browser.
 */

'use strict';
import * as DGram from 'dgram';
import * as Net from 'net';
import * as vscode from 'vscode';
import { randomBytes } from 'crypto';
import { tmpdir } from 'os';
import { join } from 'path';
import { platform } from 'process';
import { ProviderResult } from 'vscode';
import { MockDebugSession } from './mockDebug';
import { activateMockDebug, workspaceFileAccessor } from './activateMockDebug';
import { DebugSession } from '@vscode/debugadapter';

/*
 * The compile time flag 'runMode' controls how the debug adapter is run.
 * Please note: the test suite only supports 'external' mode.
 */
const runMode: 'external' | 'server' | 'namedPipeServer' | 'inline' = 'external';
let socket : DGram.Socket;
const maximumActiveTime : number  = 3000;
let activeServers :Map<string, ServerInfo> = new Map<string, ServerInfo>();

class BufferReader{
	private buffer: Buffer;
    private	offset: number = 0;

	constructor(buffer:Buffer){
		this.buffer = buffer;
	}

	readString():string{
		let len = this.buffer.readInt16LE(this.offset);
		this.offset= this.offset + 2;
		let res= this.buffer.toString(undefined, this.offset, this.offset+len);
		this.offset= this.offset + len;
		return res;
	}

	readInt():number{
		let res = this.buffer.readInt32LE(this.offset);
		this.offset = this.offset + 4;
		return res;
	}
}
export class ServerInfo{
	private address : string;
	private lastActive : number;
	private project : string;
	private machineName : string;
	private processId : number;
	private port : number;
	
	constructor(msg:Buffer, rInfo : DGram.RemoteInfo){
		let reader = new BufferReader(msg);
		this.project = reader.readString();
		this.machineName = reader.readString();
		this.processId = reader.readInt();
		this.port = reader.readInt();
		this.address = rInfo.address + ":" + this.port;
		this.lastActive = Date.now();
	}

	getAddress():string{
		return this.address;
	}

	getProject():string{
		return this.project;
	}

	getMachine():string{
		return this.machineName;
	}

	getProcessId():number{
		return this.processId;
	}

	isExipired():boolean{
		let pastTime = Date.now() - this.lastActive;
		return pastTime > maximumActiveTime;
	}
}

export function activate(context: vscode.ExtensionContext) {
	let config = vscode.workspace.getConfiguration('ilruntime');
	activeServers.clear();
	socket = DGram.createSocket("udp4");
	socket.on("message", function (msg, rinfo) {
		let serverInfo = new ServerInfo(msg, rinfo);
		activeServers.set(serverInfo.getAddress(), serverInfo);
	  });
	let port = config.get("broadcastPort") as number;
	socket.bind(port);
	
    // debug adapters can be run in different ways by using a vscode.DebugAdapterDescriptorFactory:
	switch (runMode) {
		case 'server':
			// run the debug adapter as a server inside the extension and communicate via a socket
			activateMockDebug(context, activeServers, new MockDebugAdapterServerDescriptorFactory());
			break;

		case 'namedPipeServer':
			// run the debug adapter as a server inside the extension and communicate via a named pipe (Windows) or UNIX domain socket (non-Windows)
			activateMockDebug(context, activeServers, new MockDebugAdapterNamedPipeServerDescriptorFactory());
			break;

		case 'external': default:
			// run the debug adapter as a separate process
			activateMockDebug(context, activeServers, new DebugAdapterExecutableFactory());
			break;

		case 'inline':
			// run the debug adapter inside the extension and directly talk to it
			activateMockDebug(context, activeServers);
			break;
	}
}

export function deactivate() {
	// nothing to do
	if(socket !== null){
		socket.close();
	}
	activeServers.clear();	
}
export let currentSession : vscode.DebugSession;
class DebugAdapterExecutableFactory implements vscode.DebugAdapterDescriptorFactory {

	// The following use of a DebugAdapter factory shows how to control what debug adapter executable is used.
	// Since the code implements the default behavior, it is absolutely not neccessary and we show it here only for educational purpose.

	createDebugAdapterDescriptor(_session: vscode.DebugSession, executable: vscode.DebugAdapterExecutable | undefined): ProviderResult<vscode.DebugAdapterDescriptor> {
		// param "executable" contains the executable optionally specified in the package.json (if any)
		currentSession = _session;
		// use the executable specified in the package.json if it exists or determine it based on some other information (e.g. the session)
		if (!executable) {
			const command = "absolute path to my DA executable";
			const args = [
				"some args",
				"another arg"
			];
			const options = {
				cwd: "working directory for executable",
				env: { "envVariable": "some value" }
			};
			executable = new vscode.DebugAdapterExecutable(command, args, options);
		}

		// make VS Code launch the DA executable
		return executable;
	}
}

class MockDebugAdapterServerDescriptorFactory implements vscode.DebugAdapterDescriptorFactory {

	private server?: Net.Server;

	createDebugAdapterDescriptor(session: vscode.DebugSession, executable: vscode.DebugAdapterExecutable | undefined): vscode.ProviderResult<vscode.DebugAdapterDescriptor> {

		if (!this.server) {
			// start listening on a random port
			this.server = Net.createServer(socket => {
				const session = new MockDebugSession(workspaceFileAccessor);
				session.setRunAsServer(true);
				session.start(socket as NodeJS.ReadableStream, socket);
			}).listen(0);
		}

		// make VS Code connect to debug server
		return new vscode.DebugAdapterServer((this.server.address() as Net.AddressInfo).port);
	}

	dispose() {
		if (this.server) {
			this.server.close();
		}
	}
}

class MockDebugAdapterNamedPipeServerDescriptorFactory implements vscode.DebugAdapterDescriptorFactory {

	private server?: Net.Server;

	createDebugAdapterDescriptor(session: vscode.DebugSession, executable: vscode.DebugAdapterExecutable | undefined): vscode.ProviderResult<vscode.DebugAdapterDescriptor> {

		if (!this.server) {
			// start listening on a random named pipe path
			const pipeName = randomBytes(10).toString('utf8');
			const pipePath = platform === "win32" ? join('\\\\.\\pipe\\', pipeName) : join(tmpdir(), pipeName);

			this.server = Net.createServer(socket => {
				const session = new MockDebugSession(workspaceFileAccessor);
				session.setRunAsServer(true);
				session.start(<NodeJS.ReadableStream>socket, socket);
			}).listen(pipePath);
		}

		// make VS Code connect to debug server
		return new vscode.DebugAdapterNamedPipeServer(this.server.address() as string);
	}

	dispose() {
		if (this.server) {
			this.server.close();
		}
	}
}
