using System;
using ILRuntimeTest.TestBase;
using ILRuntimeTest.TestFramework;

namespace TestCases
{
    public static class NeoStep14Test
    {
        private static Exception New() => NeoStep14CLRHost.CreateException(false);
        private static Exception NewArgument() => NeoStep14CLRHost.CreateException(true);

        private static void Check(bool condition, Exception failure)
        {
            if (!condition)
                throw failure;
        }

        public static void NeoStep14BasicCatch()
        {
            var ex = New();
            NeoStep14CLRHost.AssertSame(ex, NeoStep14Targets.BasicCatch(ex));
        }

        public static void NeoStep14CatchNull()
        {
            var ex = New();
            Check(NeoStep14Targets.CatchNull(null) == 1, ex);
        }

        public static void NeoStep14CrossFrame()
        {
            var ex = New();
            NeoStep14CLRHost.AssertSame(ex, NeoStep14Targets.CrossFrame(ex));
        }

        public static void NeoStep14NestedPrecedence()
        {
            var ex = New();
            Check(NeoStep14Targets.NestedPrecedence(ex) == 1, ex);
        }

        public static void NeoStep14EmptyCatch()
        {
            var ex = New();
            Check(NeoStep14Targets.EmptyCatch(ex) == 7, ex);
        }

        public static void NeoStep14FinallyNormal()
        {
            var ex = New();
            Check(NeoStep14Targets.FinallyNormal(1) == 12, ex);
        }

        public static void NeoStep14NestedLeave()
        {
            var ex = New();
            Check(NeoStep14Targets.NestedLeave(1) == 123, ex);
        }

        public static void NeoStep14ExceptionalFinally()
        {
            var ex = New();
            Check(NeoStep14Targets.ExceptionalFinally(ex) == 123, ex);
        }

        public static void NeoStep14Replacement()
        {
            var ex = New();
            var replacement = NewArgument();
            NeoStep14CLRHost.AssertSame(replacement, NeoStep14Targets.Replacement(ex, replacement));
        }

        public static void NeoStep14FinallyNestedCatch()
        {
            var ex = New();
            var nested = NewArgument();
            bool caught = false;
            NeoStep14Targets.ResetTrace(0);

            try
            {
                NeoStep14Targets.FinallyNestedCatch(ex, nested);
            }
            catch (Exception actual)
            {
                NeoStep14CLRHost.AssertSame(ex, actual);
                caught = true;
            }

            Check(caught && NeoStep14Targets.ReadTrace(0) == 45, ex);
        }

        public static void NeoStep14FinallyNestedFinally()
        {
            var ex = New();
            Check(NeoStep14Targets.FinallyNestedFinally(1) == 1234, ex);
        }

        public static void NeoStep14FinallyReturn()
        {
            var ex = New();
            Check(NeoStep14Targets.FinallyReturn(42) == 42 && NeoStep14Targets.ReadTrace(0) == 9, ex);
        }

        public static void NeoStep14FourArgumentCall()
        {
            var ex = New();
            Check(NeoStep14Targets.FourArgumentCall(ex) == 45, ex);
        }

        public static void NeoStep14ConstructorFailure()
        {
            var ex = New();
            Check(NeoStep14Targets.ConstructorFailure(ex) == 7, ex);
        }

        public static void NeoStep14CatchMessage()
        {
            var ex = New();
            Check(NeoStep14Targets.CatchMessage(ex) == ex.Message, ex);
        }

        public static void NeoStep14LocalPreservation()
        {
            var ex = New();
            Check(NeoStep14Targets.LocalPreservation(ex) == 54 + ex.Message.Length, ex);
        }

        public static void NeoStep14CatchTypeOrder()
        {
            var ex = New();
            Check(NeoStep14Targets.CatchTypeOrder(NewArgument()) == 1 && NeoStep14Targets.CatchTypeOrder(ex) == 2, ex);
        }

        public static void NeoStep14CrossFrameFinally()
        {
            var ex = New();
            Check(NeoStep14Targets.CrossFrameFinally(ex) == 123, ex);
        }

        public static void NeoStep14LeaveReplacement()
        {
            var ex = New();
            var replacement = NewArgument();
            NeoStep14CLRHost.AssertSame(replacement, NeoStep14Targets.LeaveReplacement(replacement));
            Check(NeoStep14Targets.ReadTrace(0) == 1, ex);
        }

        // These checks need CLR control of the external call boundary or hand-authored IL.
        public static void NeoStep14Rethrow()
        {
            NeoStep14CLRHost.Test(TestSession.LastSession.Appdomain, "Rethrow");
        }

        public static void NeoStep14NestedCatchRethrow()
        {
            NeoStep14CLRHost.Test(TestSession.LastSession.Appdomain, "NestedCatchRethrow");
        }

        public static void NeoStep14NoMatchingCatch()
        {
            NeoStep14CLRHost.Test(TestSession.LastSession.Appdomain, "NoMatchingCatch");
        }

        public static void NeoStep14FinallyNestedRethrow()
        {
            NeoStep14CLRHost.Test(TestSession.LastSession.Appdomain, "FinallyNestedRethrow");
        }

        public static void NeoStep14CatchCallsHandledMethod()
        {
            NeoStep14CLRHost.Test(TestSession.LastSession.Appdomain, "CatchCallsHandledMethod");
        }

        public static void NeoStep14ReuseInterpreter()
        {
            NeoStep14CLRHost.Test(TestSession.LastSession.Appdomain, "ReuseInterpreter");
        }

        public static void NeoStep14RejectClauses()
        {
            NeoStep14CLRHost.Test(TestSession.LastSession.Appdomain, "RejectClauses");
        }

        public static void NeoStep14HandlerAtMethodEnd()
        {
            NeoStep14CLRHost.Test(TestSession.LastSession.Appdomain, "HandlerAtMethodEnd");
        }
    }

    public static class NeoStep14Targets
    {
        private static int trace;

        public static int ReadTrace(int unused)
        {
            return trace;
        }

        public static int ResetTrace(int unused)
        {
            trace = 0;
            return 0;
        }

        public static int Identity(int value)
        {
            return value;
        }

        public static void ThrowHelper(Exception ex)
        {
            throw ex;
        }

        public static object BasicCatch(Exception ex)
        {
            try
            {
                throw ex;
            }
            catch (Exception caught)
            {
                return caught;
            }
        }

        public static int CatchNull(Exception unused)
        {
            try
            {
                throw null;
            }
            catch (NullReferenceException)
            {
                return 1;
            }
        }

        public static object CrossFrame(Exception ex)
        {
            try
            {
                ThrowHelper(ex);
            }
            catch (Exception caught)
            {
                return caught;
            }

            return null;
        }

        public static int NestedPrecedence(Exception ex)
        {
            try
            {
                try
                {
                    ThrowHelper(ex);
                }
                catch (Exception)
                {
                    return 1;
                }
            }
            catch (InvalidOperationException)
            {
                return 2;
            }

            return 0;
        }

        public static int EmptyCatch(Exception ex)
        {
            int result = 0;
            try
            {
                ThrowHelper(ex);
            }
            catch
            {
                result = 7;
            }

            return result;
        }

        public static int FinallyNormal(int value)
        {
            trace = 0;
            try
            {
                trace = value;
            }
            finally
            {
                trace = trace * 10 + 2;
            }

            return trace;
        }

        public static int NestedLeave(int value)
        {
            trace = 0;
            try
            {
                try
                {
                    trace = value;
                }
                finally
                {
                    trace = trace * 10 + 2;
                }
            }
            finally
            {
                trace = trace * 10 + 3;
            }

            return trace;
        }

        public static int ExceptionalFinally(Exception ex)
        {
            trace = 0;
            try
            {
                try
                {
                    try
                    {
                        ThrowHelper(ex);
                    }
                    finally
                    {
                        trace = trace * 10 + 1;
                    }
                }
                finally
                {
                    trace = trace * 10 + 2;
                }
            }
            catch (Exception)
            {
                trace = trace * 10 + 3;
            }

            return trace;
        }

        public static void Rethrow(Exception ex)
        {
            try
            {
                NeoStep14CLRHost.ThrowFromCLR(ex);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void NestedCatchRethrow(Exception ex, Exception nested)
        {
            try
            {
                NeoStep14CLRHost.ThrowFromCLR(ex);
            }
            catch (Exception)
            {
                try
                {
                    ThrowHelper(nested);
                }
                catch (Exception)
                {
                    trace = 1;
                }

                throw;
            }
        }
        public static object Replacement(Exception ex, Exception replacement)
        {
            try
            {
                try
                {
                    ThrowHelper(ex);
                }
                finally
                {
                    ThrowHelper(replacement);
                }
            }
            catch (Exception caught)
            {
                return caught;
            }

            return null;
        }

        public static void FinallyNestedCatch(Exception ex, Exception nested)
        {
            try
            {
                ThrowHelper(ex);
            }
            finally
            {
                try
                {
                    ThrowHelper(nested);
                }
                catch (Exception)
                {
                    trace = 4;
                }

                trace = trace * 10 + 5;
            }
        }

        public static int FinallyNestedFinally(int value)
        {
            trace = 0;
            try
            {
                trace = value;
            }
            finally
            {
                try
                {
                    trace = trace * 10 + 2;
                }
                finally
                {
                    trace = trace * 10 + 3;
                }

                trace = trace * 10 + 4;
            }

            return trace;
        }

        public static int FinallyReturn(int value)
        {
            try
            {
                return value;
            }
            finally
            {
                trace = 9;
            }
        }

        public static int FourArgumentCall(Exception ex)
        {
            trace = 0;
            try
            {
                ThrowFour(1, 2, 3, ex);
            }
            catch (Exception)
            {
                trace = 4;
            }
            finally
            {
                trace = trace * 10 + 5;
            }

            return trace;
        }

        private static void ThrowFour(int a, int b, int c, Exception ex)
        {
            trace = a + b + c;
            throw ex;
        }

        public static int ConstructorFailure(Exception ex)
        {
            var value = new NeoStep14Box(7, null);
            try
            {
                value = new NeoStep14Box(9, ex);
            }
            catch (Exception)
            {
            }

            return value.Value;
        }

        public static string CatchMessage(Exception ex)
        {
            try
            {
                ThrowHelper(ex);
            }
            catch (Exception caught)
            {
                return caught.Message;
            }

            return null;
        }

        public static int LocalPreservation(Exception ex)
        {
            string saved = "kept";
            int number = 3;
            try
            {
                number = 5;
                ThrowHelper(ex);
            }
            catch (Exception caught)
            {
                return number * 10 + saved.Length + caught.Message.Length;
            }

            return 0;
        }

        public static int CatchTypeOrder(Exception ex)
        {
            try
            {
                ThrowHelper(ex);
            }
            catch (ArgumentException)
            {
                return 1;
            }
            catch (Exception)
            {
                return 2;
            }

            return 0;
        }

        public static object NoMatchingCatch(Exception ex)
        {
            try
            {
                ThrowHelper(ex);
            }
            catch (ArgumentException caught)
            {
                return caught;
            }

            return null;
        }

        private static void FinallyCallee(Exception ex)
        {
            try
            {
                ThrowHelper(ex);
            }
            finally
            {
                trace = trace * 10 + 1;
            }
        }

        public static int CrossFrameFinally(Exception ex)
        {
            trace = 0;
            try
            {
                try
                {
                    FinallyCallee(ex);
                }
                finally
                {
                    trace = trace * 10 + 2;
                }
            }
            catch (Exception)
            {
                trace = trace * 10 + 3;
            }

            return trace;
        }

        public static object LeaveReplacement(Exception replacement)
        {
            try
            {
                try
                {
                    trace = 1;
                }
                finally
                {
                    ThrowHelper(replacement);
                }

                trace = 99;
            }
            catch (Exception caught)
            {
                return caught;
            }

            return null;
        }

        public static void FinallyNestedRethrow(Exception ex, Exception nested)
        {
            try
            {
                ThrowHelper(ex);
            }
            finally
            {
                try
                {
                    NeoStep14CLRHost.ThrowFromCLR(nested);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public static void CatchCallsHandledMethod(Exception ex, Exception nested)
        {
            try
            {
                NeoStep14CLRHost.ThrowFromCLR(ex);
            }
            catch (Exception)
            {
                BasicCatch(nested);
                throw;
            }
        }

        public static object Unsupported(int unused)
        {
            // CLR reference constructors are supported since Step14.5.
            return new DateTime(2020, 1, 1);
        }
    }

    public class NeoStep14Box
    {
        public int Value;

        public NeoStep14Box(int value, Exception ex)
        {
            Value = value;
            if (value == 9)
                throw ex;
        }
    }
}
