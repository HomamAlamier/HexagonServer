using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
namespace HexagonServer
{

    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            server.init();
        }
    }
    public class Server
    {
        public struct player
        {
            public string name;
            public bool online;
            public int timeout;
            public string ip;
            public string port;
            public bool playing;
            public string playingIp;
            public bool host;
            public string gamepath;
        }
        public struct retIP
        {
            public string ip, port;
        }
        static player[] db = new player[10];
        Socket sc;
        int port = 21;
        int backlog = 5;
        byte[] buffer = new byte[1024];
        static int index = 0;
        public Server()
        {
            sc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            for (int i = 0; i < db.Length; i++)
            {
                db[i] = new player();
            }
            //tools.getRegFix("notepad.exe");
        }
        public void init()
        {
            try
            {
                //SocketPermission sp = new SocketPermission(NetworkAccess.Accept, TransportType.All, "", SocketPermission.AllPorts);
                //sp.Demand();
                sc.Bind(new IPEndPoint(IPAddress.Any, port));
                sc.Listen(backlog);
                Accept();
                Console.WriteLine("Inited !");
                while (true)
                {

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public void Accept()
        {
            sc.BeginAccept(new AsyncCallback(AcceptCallBack), null);
        }

        private void AcceptCallBack(IAsyncResult ar)
        {
            try
            {
                Socket client = sc.EndAccept(ar);
                //Console.WriteLine("Client Accepted !");
                client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), client);
                Accept();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void ReceiveCallBack(IAsyncResult ar)
        {
            try
            {
                Socket temp = (Socket)ar.AsyncState;
                int size = temp.EndReceive(ar);
                byte[] Rbuffer = new byte[size];
                Array.Copy(buffer, Rbuffer, size);
                string data = Encoding.ASCII.GetString(Rbuffer);
                Console.WriteLine("-----Requst----------------------------------------------------------");
                Console.WriteLine(data);
                IPEndPoint rIp = temp.RemoteEndPoint as IPEndPoint;
                string html = tools.CheckGET(data, rIp.Address.ToString(), rIp.Port.ToString());
                Console.WriteLine("-----Response----------------------------------------------------------");
                Console.WriteLine(html);
                byte[] response = null;
                if (html.IndexOf("getfile:") > -1)
                {
                    Console.WriteLine(html);
                    int n = html.IndexOf("getfile:");
                    string path = html.Substring(n + 8, html.Length - (n + 8));
                    response = tools.GetFileBytes(path);
                }
                else
                    response = Encoding.UTF8.GetBytes(html);
                temp.BeginSend(response, 0, response.Length, SocketFlags.None, new AsyncCallback(SendCallBack), temp);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public static bool isPlaying(string name)
        {
            for (int i = 0; i < index; i++)
            {
                if (db[i].name == name) return db[i].playing;
            }
            return false;
        }
        public static bool isHost(string ip)
        {
            for (int i = 0; i < index; i++)
            {
                if (db[i].ip == ip) return db[i].host;
            }
            return false;
        }
        public static retIP getIPPORT(string name)
        {
            retIP temp = new retIP();
            for (int i = 0; i < index; i++)
            {
                if (db[i].name == name)
                {
                    //temp = new retIP();
                    temp.ip = db[i].ip;
                    temp.port = db[i].port;
                    return temp;
                }
            }
            return temp;
        }
        public static string getNameFromIP(string ip)
        {
            for (int i = 0; i < index; i++)
            {
                if (db[i].ip == ip) return db[i].name;
            }
            return null;
        }
        public static bool IsTaken(string name, string ip)
        {
            for (int i = 0; i < index; i++)
            {
                if (db[i].name == name && db[i].ip == ip)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool isOnline(string id)
        {
            for (int i = 0; i < db.Length; i++)
            {
                if (db[i].name == id && db[i].online == true)
                {
                    db[i].timeout = 5;
                    return true;
                }
            }
            return false;
        }
        public static int getOnline()
        {
            int temp = 0;
            for (int i = 0; i < db.Length; i++)
            {
                if (db[i].online == true)
                {
                    temp++;
                }
            }
            return temp;
        }
        public static string[] getOnlineList()
        {
            string[] temp = new string[getOnline()];
            int ind = 0;
            for (int i = 0; i < db.Length; i++)
            {
                if (db[i].online == true)
                {
                    temp[ind] = db[i].name;
                    ind++;
                }
            }
            return temp;
        }
        public static void setOnline(string id)
        {
            for (int i = 0; i < db.Length; i++)
            {
                if (db[i].name == id)
                {
                    db[i].online = true;
                }
            }
        }
        public static void setPlaying(string ip, string destIp, bool host)
        {
            for (int i = 0; i < index; i++)
            {
                if (db[i].ip == ip)
                {
                    db[i].playingIp = destIp;
                    db[i].playing = true;
                    db[i].host = host;
                    return;
                }
            }
        }
        public static void setPlaying(string ip, bool playing)
        {
            for (int i = 0; i < index; i++)
            {
                if (db[i].ip == ip)
                {
                    db[i].playing = playing;
                    return;
                }
            }
        }
        public static string getPlayingIp(string ip)
        {
            for (int i = 0; i < index; i++)
            {
                if (db[i].ip == ip) return db[i].playingIp;
            }
            return null;
        }
        public static void addPlayer(string name, string ip, string port, string path)
        {
            db[index].name = name;
            db[index].online = true;
            db[index].timeout = 5;
            db[index].ip = ip;
            db[index].port = port;
            db[index].gamepath = path;
            db[index].playingIp = "";
            db[index].playing = false;
            db[index].host = false;
            index++;
        }
        private void SendCallBack(IAsyncResult ar)
        {
            try
            {
                Socket temp = (Socket)ar.AsyncState;
                temp.Shutdown(SocketShutdown.Send);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
    public class tools
    {
        public static string GetIndex()
        {
            byte[] temp = null;
            System.IO.FileStream f = new System.IO.FileStream("html/index.html", System.IO.FileMode.Open);
            temp = new byte[f.Length];
            f.Read(temp, 0, (int)f.Length);
            f.Close();
            return Encoding.ASCII.GetString(temp);
        }
        public static string GetFile(string fn)
        {
            byte[] temp = null;
            System.IO.FileStream f = new System.IO.FileStream("html/" + fn, System.IO.FileMode.Open);
            temp = new byte[f.Length];
            f.Read(temp, 0, (int)f.Length);
            f.Close();
            return Encoding.ASCII.GetString(temp);
        }
        public static void GetFile(string name, string contant)
        {
            if (System.IO.File.Exists("html/" + name) == true)
            {
                System.IO.File.Delete("html/" + name);
            }
            System.IO.FileStream f = new System.IO.FileStream("html/" + name, System.IO.FileMode.Create);
            byte[] fn = Encoding.ASCII.GetBytes(contant);
            f.Write(fn, 0, fn.Length);
            f.Close();
        }
        public static byte[] GetFileBytes(string name)
        {
            //retry:
            try
            {
                System.IO.FileStream f = new System.IO.FileStream("html/" + name, System.IO.FileMode.Open);
                byte[] fn = new byte[f.Length];
                f.Read(fn, 0, (int)f.Length);
                f.Close();
                return fn;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }
        public static string CheckGET(string get, string ip, string port)
        {
            int n = get.IndexOf("/");
            string s = get.Substring(n + 1, 1);
            if (s == "?")
            {
                n++;
                int e = get.IndexOf("HTTP");
                GetResponse(get.Substring(n + 1, e - (n + 1)));
            }
            else
            {
                int y = get.IndexOf("?");
                if (y > -1)
                {
                    if (get.Substring(n + 1, y - (n + 1)) == "login")
                    {
                        n += 6;
                        int e = get.IndexOf("HTTP");
                        string o = get.Substring(n + 1, e - (n + 2));
                        string nm = GetResponse(o, true, "name");
                        Console.WriteLine("Login requste by the name of [" + nm + "] form ip [" + ip + "]");
                        if (Server.IsTaken(nm, ip) == false)
                        {
                            string ph = GetResponse(o, true, "path");
                            ph = ConvertFromHtml(ph);
                            Server.addPlayer(nm, ip, port, ph);
                            Console.WriteLine("Registered !");
                            Console.WriteLine("Creating registry key...");
                            tools.GetFile("keys/" + nm + "-FixKey.reg", tools.getRegFix(ph));
                            Console.WriteLine("Finished.");
                            return "<html><head><meta http-equiv=" + '"' + "refresh" + '"' + " content=" + '"' +
                                "1;url=online.html" + '"' + "><a href=" + '"' + "online.html" + '"' + ">Go To Lobby</a></head></html>";
                        }
                        else
                        {
                            Console.WriteLine("The name is already taken from this ip");
                            return GetFile("loginError.html");
                        }
                    }
                    if (get.Substring(n + 1, y - (n + 1)) == "connect")
                    {
                        n += 8;
                        int e = get.IndexOf("HTTP");
                        string o = get.Substring(n + 1, e - (n + 2));
                        string p2 = GetResponse(o, true, "player");
                        Server.retIP p2Ip = Server.getIPPORT(p2);
                        string p1 = Server.getNameFromIP(ip);
                        Console.WriteLine(p1 + " (" + ip + ") --> " + p2 + " (" + p2Ip.ip + ")");
                        Server.setPlaying(ip, p2Ip.ip, true);
                        Server.setPlaying(p2Ip.ip, ip, false);
                        return "<html><head><meta http-equiv=" + '"' + "refresh" + '"' + " content=" + '"' +
                                 "0;url=online.html" + '"' + ">" +
                                 "</head></html>";
                    }
                }
            }
            string t = get.Substring(n + 1, get.IndexOf("HTTP") - 6);
            // Console.WriteLine(t);
            if (t.IndexOf(".css") > -1 || t.IndexOf(".html") > -1)
                if (t == "online.html")
                    return getOnlinePlayerPage(ip);
                else
                    return GetResponse(t);
            else if (t.IndexOf(".reg") > -1) return GetFile(t);
            else if (t.IndexOf(".zip") > -1 || t.IndexOf(".ico") > -1) return "getfile:" + t;
            else
            {
                if (Server.IsTaken(Server.getNameFromIP(ip), ip) == true)
                {
                    string toOnline = "<html><head><meta http-equiv=" + '"' + "refresh" + '"' + " content=" + '"' +
                "0;url=online.html" + '"' + "></head></html>"; ;
                }
                else return GetIndex();
            }
            return "";
        }

        string getHtml(string url, int delay)
        {
            string html = "<html><head><script type=\"text / javascript\">window.open('" + url + "','_blank');</script>" +
                "<meta http-equiv=" + '"' + "refresh" + '"' + " content=" + '"' +
                delay.ToString() + ";url=online.html" + '"' + ">" +
                "</head>The Game Is starting in " + delay.ToString() + " seconds...</html>";
            return html;
        }
        private static string ConvertFromHtml(string val)
        {
            string ph = val;
            int n = 0;
            do
            {
                n = ph.IndexOf("%5C", n + 1);
                if (n > -1)
                {
                    ph = ph.Remove(n, 3);
                    ph = ph.Insert(n, "\\");
                }
            } while (n > -1);
            n = 0;
            do
            {
                n = ph.IndexOf("+", n + 1);
                if (n > -1)
                {
                    ph = ph.Remove(n, 1);
                    ph = ph.Insert(n, " ");
                }
            } while (n > -1);
            n = 0;
            do
            {
                n = ph.IndexOf("%3A");
                if (n > -1)
                {
                    ph = ph.Remove(n, 3);
                    ph = ph.Insert(n, ":");
                }
            } while (n > -1);
            return ph;
        }

        private static string getOnlinePlayerPage(string ip)
        {
            string html = GetFile("online.html");
            string[] op = Server.getOnlineList();
            string body = "";
            for (int i = 0; i < op.Length; i++)
            {
                if (op[i] != Server.getNameFromIP(ip)) body += "<input type=" + '"' + "radio" + '"' + " value=" + '"' + op[i] + '"' + " name=" + '"' + "player" + '"' + ">" + op[i] + "<br>";
            }
            int n0 = html.IndexOf("{0}");
            html = html.Remove(n0, 3);
            html = html.Insert(n0, Server.getOnline().ToString());
            int n1 = html.IndexOf("{1}");
            html = html.Remove(n1, 3);
            html = html.Insert(n1, body);
            int n2 = html.IndexOf("{2}");
            html = html.Remove(n2, 3);
            html = html.Insert(n2, "keys/" + Server.getNameFromIP(ip) + "-FixKey.reg");
            int n3 = html.IndexOf("{3}");
            html = html.Remove(n3, 3);
            int n4;
            if (Server.isPlaying(Server.getNameFromIP(ip)) == false)
            {
                html = html.Insert(n3, "0");
            }
            else
            {
                html = html.Insert(n3, "1");
                n4 = html.IndexOf("{4}");
                html = html.Remove(n4, 3);
                if (Server.isHost(ip) == true)
                    html = html.Insert(n4, "hexagame://-h " + ip);
                else
                    html = html.Insert(n4, "hexagame://-c " + Server.getPlayingIp(ip));
                Server.setPlaying(ip, false);
            }
            return html;
        }

        public static string getRegFix(string path)
        {
            string regFile = GetFile("RegURLFIX.reg");
            int pInd = regFile.IndexOf("{0}");
            regFile = regFile.Remove(pInd, 3);
            regFile = regFile.Insert(pInd, path);
            return regFile;
        }
        public static string GetResponse(string data, bool valonly = false, string attrib = "")
        {
            string res = "";
            int n = data.IndexOf("=", data.IndexOf(attrib));
            int s = data.IndexOf(attrib);
            if (data.IndexOf(".css") > -1 || data.IndexOf(".html") > -1)
                return GetFile(data);
            if (n > -1)
            {
                do
                {
                    string attr = data.Substring(s, n - (s));
                    string val = "";
                    int x = data.IndexOf("=", n + 1);
                    if (data.IndexOf("&", s) > -1) val = data.Substring(n + 1, data.IndexOf("&") - (n + 1));
                    else val = data.Substring(n + 1, data.Length - (n + 1));
                    if (valonly == false) res += attr + " : " + val;
                    else res += val + ";";
                    n = x;
                    if (attr == "name")
                    {
                        if (Server.isOnline(val) == false)
                        {
                            Server.setOnline(val);
                        }
                    }
                    if (valonly == true)
                    {
                        if (attr == attrib)
                        {
                            return val;
                        }
                    }
                } while (n > -1);
                return res;
            }
            else
            {
                return GetIndex();
            }
        }
    }
}
