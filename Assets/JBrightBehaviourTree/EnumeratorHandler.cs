using System;
using System.Collections;
using UnityEngine;

namespace Plugins.JBrightBehaviourTree
{
    public static class EnumeratorHandler
    {
        public static bool ConsumeEnumerator(IEnumerator enumerator)
        {
            var current = enumerator.Current;

            if (current == null)
            {
                if (enumerator.MoveNext() == false)
                    return false;
            }
            else if (current is CustomYieldInstruction cyi)
            {
                if (ConsumeCustomYieldInstruction( cyi ) == false && enumerator.MoveNext() == false)
                    return false;
            }
            else if (current is YieldInstruction yieldInstruction)
            {
                if (ConsumeYieldInstruction( yieldInstruction ) == false && enumerator.MoveNext() == false)
                    return false;
            }
            else if (current is IEnumerator innerEnumerator)
            {
                if (ConsumeInnerEnumerator( innerEnumerator ) == false && enumerator.MoveNext() == false)
                    return false;
            }
            else
            {
                PrintWarn( current );
                if (enumerator.MoveNext() == false)
                    return false;
            }

            return true;
        }

        private static bool ConsumeInnerEnumerator(IEnumerator innerEnumerator)
        {
            return ConsumeEnumerator( innerEnumerator );
        }

        private static bool ConsumeYieldInstruction(YieldInstruction yieldInstruction)
        {
            switch (yieldInstruction)
            {
                case AsyncOperation ao:
                    return ao.isDone == false;
                default:
                    PrintWarn( yieldInstruction );
                    return false;
            }
        }

        private static bool ConsumeCustomYieldInstruction(CustomYieldInstruction cyi)
        {
            if (cyi.keepWaiting)
                return true;
            return false;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        static void PrintWarn(object current)
        {
            throw new Exception( $"yield {current.GetType().Name} is not supported." );
        }
    }
}