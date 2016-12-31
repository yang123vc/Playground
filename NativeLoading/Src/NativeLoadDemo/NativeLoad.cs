using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SciterSharp;

namespace NativeLoadDemo
{
    public class NativeLoad : SciterEventHandler
    {
		public bool Host_HelloWorld(SciterElement el, SciterValue[] args, out SciterValue result)
		{
			result = new SciterValue("Hello World! (from native behavior)");
			return true;
		}
    }
}
