using System;
using System.Windows.Forms;
using System.Reflection;
using System.Resources;

namespace RSDiagnostics
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ResolveEmbededAssembly); 
            Application.Run(new MainForm());
        }


        // Give credit where credit is due: https://stackoverflow.com/a/6362414
        static Assembly ResolveEmbededAssembly(object sender, ResolveEventArgs args)
        {
            string dllName = args.Name.Contains(",") ? args.Name.Substring(0, args.Name.IndexOf(',')) : args.Name.Replace(".dll", "");

            dllName = dllName.Replace(".", "_").Replace("-", "_");

            if (dllName.EndsWith("_resources"))
                return null;

            ResourceManager rm = new ResourceManager(new object().GetType().Namespace + ".Properties.Resources", Assembly.GetExecutingAssembly());

            byte[] bytes = (byte[])rm.GetObject(dllName);

            return Assembly.Load(bytes);
        }
    }
}
