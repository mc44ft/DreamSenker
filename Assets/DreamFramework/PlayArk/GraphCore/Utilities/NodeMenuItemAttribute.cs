using System;

namespace PlayArk.GraphCore.Utilities
{
    //参数（该特性只允许写在类上面，该特性不能对同一个类重复使用，该特性不被类的子类继承）
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class NodeMenuItemAttribute : System.Attribute
    {
        public string MenuTitle { get; private set; }
        public NodeMenuItemAttribute(string menuTitle)
        {
            MenuTitle = menuTitle;
        }
    }
}

