using System;
using System.Diagnostics;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Westwind.Utilities;

namespace MarkdownMonster
{
    /// <summary>
    /// Base COM Interop class used for browser interop.
    ///
    /// Uses an internal Instance property to hold COM reference that's used as a base for other operations.
    /// </summary>
    public class BaseBrowserInterop {
        

        /// <summary>
        /// The actual raw COM instance of the `te` instance
        /// inside of  `editor.js`. The internal members use
        /// this instance to access the members of the underlying
        /// JavaScript object.
        ///
        /// You can use ReflectionUtils to further use Reflection
        /// on this instance to automate Ace Editor.
        ///
        /// Example:
        /// var udm = ReflectionUtils.InvokeMethod(AceEditor.Instance,"editor.session.getUndoManager",false)
        /// RelectionManager.Invoke(udm,"undo",false);
        ///
        /// Note methods with no parameters should pass `false`
        /// </summary>
        public object Instance
        {
            get => _instance;
            set
            {
                _instance = value;
                if (_instance != null)
                    InstanceType = _instance.GetType();
            }
        }
        private object _instance;
        public Type InstanceType { get; set; }



        public BaseBrowserInterop(object instance)
        {
            Instance = instance;
        }

        public BaseBrowserInterop()
        {

        }

        /// <summary>
        /// Helper method that consistently serializes JavaScript with Camelcase
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string SerializeObject(object data)
        {
            var settings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };
#if DEBUG
            settings.Formatting = Formatting.Indented;
#endif
            return JsonConvert.SerializeObject(data, settings);
        }


        private const BindingFlags flags =
            BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.Static | BindingFlags.Instance |
            BindingFlags.IgnoreCase;

        /// <summary>
        /// Invokes a method on the editor by name with parameters
        /// </summary>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object Invoke(string method, params object[] parameters)
        {
            // Instance methods have to have a parameter to be found (arguments array)
            if (parameters == null)
                parameters = new object[] {false};
#if DEBUG
            try
            {
#endif
            return InstanceType.InvokeMember(method, flags | BindingFlags.InvokeMethod, null, Instance,
                parameters );
#if DEBUG
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.GetBaseException().Message);
                throw ex;
            }
#endif
        }


        /// <summary>
        /// Invokes a method on the editor by name with parameters
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public T Invoke<T>(string method, params object[] parameters)
        {
            // Instance methods have to have a parameter to be found (arguments array)
            if (parameters == null)
                parameters = new object[] {false};

            //var res = ReflectionUtils.CallMethod(Instance, method, parameters);
            var res = InstanceType.InvokeMember(method, flags | BindingFlags.InvokeMethod, null, Instance,
                new object[] { parameters });
            if (res == null || res == DBNull.Value)
                return default(T);

            return (T)res;
        }

        /// <summary>
        /// Extended method invocation that can use . syntax to walk
        /// an object property hierarchy. Slower, but provides support
        /// for . and indexer [] syntax.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public object InvokeEx(string method, params object[] parameters)
        {
            // Instance methods have to have a parameter to be found (arguments array)
            if (parameters == null)
                parameters = new object[] {false};

            return ReflectionUtils.CallMethodExCom(Instance, method, parameters);
        }


        /// <summary>
        /// Extended method invocation that can use . syntax to walk
        /// an object property hierarchy.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="method"></param>
        /// <param name="parameters">optional list of parameters. Leave blank if no parameters</param>
        /// <returns></returns>
        public object InvokeEx(object instance, string method, params object[] parameters)
        {
            return ReflectionUtils.CallMethodExCom(instance, method, parameters);
        }


        /// <summary>
        /// Retrieves a property value from the editor by name
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public object Get(string propertyName)
        {
            return InstanceType.InvokeMember(propertyName, flags | BindingFlags.GetProperty, null, Instance, null);
        }

        /// <summary>
        /// Retrieves a property from an object instance
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public object Get(object instance, string propertyName)
        {
            return InstanceType.InvokeMember(propertyName, flags | BindingFlags.GetProperty, null, instance, null);
        }


        /// <summary>
        /// Retrieves a property from the editor by name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public T Get<T>(string propertyName)
        {
            var res = InstanceType.InvokeMember(propertyName, flags | BindingFlags.GetProperty, null, Instance, null);
            if (res == null || res == DBNull.Value)
                return default(T);

            return (T) res;
        }

        /// <summary>
        /// Get a property from the instance with . or [] sytnax down the tree.
        /// </summary>
        /// <param name="propertyName"></param>
        public object GetEx(string propertyName)
        {
            return ReflectionUtils.GetPropertyExCom(Instance, propertyName);
        }

        /// <summary>
        /// Retrieves a property from an object instance with . or [] syntax
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public object GetEx(object instance, string propertyName)
        {
            return ReflectionUtils.GetPropertyExCom(instance, propertyName);
        }

        /// <summary>
        /// Get a property from the instance with . or [] sytnax down the tree.
        /// </summary>
        /// <param name="propertyName"></param>
        public T GetEx<T>(string propertyName)
        {
            object result = ReflectionUtils.GetPropertyExCom(Instance, propertyName);
            if (result == null || result == DBNull.Value)
                return default(T);

            return (T)result;
        }


        /// <summary>
        /// Sets a property on the editor by name
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public void Set(string propertyName, object value)
        {
            InstanceType.InvokeMember(propertyName, flags | BindingFlags.SetProperty, null, Instance, new [] { value });
            //ReflectionUtils.SetProperty(Instance, property, value);
        }

        /// <summary>
        /// Set a property from the instance with . or [] sytnax down the tree.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public void SetEx(string propertyName, object value)
        {
            ReflectionUtils.SetPropertyExCom(Instance, propertyName, value);
        }
    }
}
