using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace SmtpMockWeb.Code
{
    public class Helper
    {
        public static string GetCurrentDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public static bool IsUserAdministrator()
        {
            bool isAdmin;
            try
            {
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException ex)
            {
                isAdmin = false;
            }
            catch (Exception ex)
            {
                isAdmin = false;
            }
            return isAdmin;
        }

        public static List<string> GetAllIps()
        {
            List<string> result = new List<string>();
            foreach (NetworkInterface netInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                IPInterfaceProperties ipProps = netInterface.GetIPProperties();
                foreach (UnicastIPAddressInformation addr in ipProps.UnicastAddresses)
                {
                    if (addr.Address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        continue;
                    }
                    result.Add(addr.Address.ToString());

                }
            }
            return result;
        }

        public static string ConvertToVirtualPath(string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return String.Empty;
            }
            if (value[0] == '/')
            {
                value = value.Substring(1);
            }
            value = value.Replace('/', '\\');
            return value;
        }

        public static string GetProductVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            if (fvi.ProductPrivatePart > 0)
            {
                return String.Format("{0}.{1}.{2}.{3}",
                    fvi.FileMajorPart, fvi.FileMinorPart, fvi.FileBuildPart, fvi.ProductPrivatePart);
            }
            if (fvi.ProductBuildPart > 0)
            {
                return String.Format("{0}.{1}.{2}",
                    fvi.FileMajorPart, fvi.FileMinorPart, fvi.FileBuildPart);
            }
            return String.Format("{0} {1}.{2}",
                fvi.ProductName, fvi.FileMajorPart, fvi.FileMinorPart);
        }
    }
}
