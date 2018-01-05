using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILRuntimeDebugEngine.Expressions
{
    class Parser
    {
        Lexer lexer;
        public Parser(Lexer lexer)
        {
            this.lexer = lexer;
        }

        public EvalExpression Parse()
        {
            var curToken = lexer.GetNextToken();
            EvalExpression res = null;
            while(curToken.Type != TokenTypes.EOF)
            {
                if (res == null)
                {
                    if (curToken.Type == TokenTypes.Name)
                    {
                        res = new NameExpression(((NameToken)curToken).Content);
                    }
                    else
                        throw new NotSupportedException("Unexpected token:" + curToken.Type);
                }
                else
                {
                    if (!res.Parse(curToken, lexer))
                    {
                        switch (curToken.Type)
                        {
                            case TokenTypes.MemberAccess:
                                res = new MemberAcessExpression(res);
                                break;
                            case TokenTypes.IndexStart:
                                res = new IndexAccessExpression(res);
                                break;
                            case TokenTypes.InvocationStart:
                                res = new InvocationExpression(res);
                                break;
                            default:
                                throw new NotSupportedException("Unexpected token:" + curToken.Type);
                        }
                    }
                }
                curToken = lexer.GetNextToken();
            }
            if(res != null && !res.Completed)
            {
                throw new NotSupportedException("Unexpected token: EOF");
            }
            return res;
        }
    }
}
