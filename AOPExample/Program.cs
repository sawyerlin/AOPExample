using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using System.Collections;

namespace AOPExample
{
    class Program
    {
        static void Main(string[] args)
        {
            IUnityContainer container = new UnityContainer();
            container.AddNewExtension<Interception>();
            container.RegisterType<DAL>(new Interceptor<VirtualMethodInterceptor>(), new InterceptionBehavior<Interceptor>());

            DAL dal = container.Resolve<DAL>();

            dal.MethodForLoggingA();

            dal.MethodForLoggingB();

            dal.MethodForLoggingC();

            Console.Read();
        }
    }

    public class Interceptor : IInterceptionBehavior
    {
        public IEnumerable<Type> GetRequiredInterfaces()
        {
            return Type.EmptyTypes;
        }

        public IMethodReturn Invoke(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext)
        {
            /* Call the method that was intercepted */
            string className = input.MethodBase.DeclaringType.Name;
            string methodName = input.MethodBase.Name;
            string generic = input.MethodBase.DeclaringType.IsGenericType ? string.Format("<{0}>", input.MethodBase.DeclaringType.GetGenericArguments().ToStringList()) : string.Empty;
            string arguments = input.Arguments.ToStringList();

            string preMethodMessage = string.Format("{0}{1}.{2}({3})", className, generic, methodName, arguments);
            Console.WriteLine("-------------------------------------");
            Console.WriteLine("PreMethodCalling: " + preMethodMessage);
            //Logging
            Logger.Instance.Log(preMethodMessage);
            //Invoke method
            IMethodReturn msg = getNext()(input, getNext);

            // Exception
            if (msg.Exception != null)
            {
                List<CustomMessage> customMessages = ((CustomMessage[])input.MethodBase.GetCustomAttributes(typeof(CustomMessage), true)).ToList();
                CustomMessage customMessage = customMessages.FirstOrDefault(attr => attr.ExceptionType == msg.Exception.GetType());
                if (customMessage != null)
                {
                    string exceptionMessage = string.Format("{0}{1}.{2}() -> {3}", className, generic, methodName,
                                                            customMessage.Message);
                    Console.WriteLine("ExceptionMessage: " + exceptionMessage);
                    //Logging
                    Logger.Instance.Log(exceptionMessage);
                }
                else
                {
                    string exceptionMessage = string.Format("{0}{1}.{2}() -> {3}", className, generic, methodName,
                                                               msg.Exception);
                    Console.WriteLine("ExceptionMessage: " + exceptionMessage);
                    //Logging
                    Logger.Instance.Log(exceptionMessage);

                }

                msg.Exception = null;
                return msg;

            }
            //Post method calling
            string postMethodMessage = string.Format("{0}{1}.{2}() -> {3}", className, generic, methodName, msg.ReturnValue);
            Console.WriteLine("PostMethodCalling: " + postMethodMessage);
            //Logging
            Logger.Instance.Log(postMethodMessage);
            return msg;
        }

        public bool WillExecute
        {
            get { return true; }
        }
    }

    public class DAL : IDAL
    {
        public virtual void MethodForLoggingA()
        {
            Console.WriteLine("Called MethodForLoggingA");
        }
        public virtual void MethodForLoggingB()
        {
            Console.WriteLine("Called MethodForLoggingB");
        }

        [CustomMessage(ExceptionType = typeof(Exception), Message = "Base Exception")]
        [CustomMessage(ExceptionType = typeof(NullReferenceException), Message = "Can't find the reference")]
        public virtual void MethodForLoggingC()
        {
            Console.WriteLine("Called MethodForLoggingC");
            throw new Exception("Test");
            //throw new NullReferenceException("Null pointer");
            //int i = 0;
            //int p = 1 / i;
            //Console.WriteLine(p.ToString());
        }
    }

    public class Logger
    {
        private static Logger _instance = new Logger();
        public static Logger Instance { get { return _instance; } }
        public void Log(string message)
        {
            //logging code
        }
    }

    public static class EnumerableExtensions
    {
        public static string ToStringList(this IEnumerable list)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var item in list)
            {
                sb.AppendFormat("{0}, ", item);
            }
            if (sb.Length > 0)
                sb.Remove(sb.Length - 2, 2);

            return sb.ToString();
        }
    }
}
