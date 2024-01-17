using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.IO;
using System.Net.Sockets;

namespace Hunk
{
    public partial class Hunk : Form
    {
        private int progreso = 0;
        public Hunk()
        {
            InitializeComponent();
            ObtenerIP();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        int m, mx, my;
        private void barra_movimiento_MouseDown(object sender, MouseEventArgs e)
        {
            m = 1;
            mx = e.X;
            my = e.Y;
        }

        private void boton_cerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void boton_minimizar_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
                WindowState = FormWindowState.Minimized;
            else if (WindowState == FormWindowState.Maximized)
                WindowState = FormWindowState.Normal;
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.LinkVisited = true;
            System.Diagnostics.Process.Start("https://github.com/rasmuskernel");
        }

        private void barra_movimiento_MouseMove(object sender, MouseEventArgs e)
        {
            if (m == 1)
            {
                this.SetDesktopLocation(MousePosition.X - mx, MousePosition.Y - my);
            }
        }

        private void barra_movimiento_MouseUp(object sender, MouseEventArgs e)
        {
            m = 0;
        }

        private void FiltrarDominio()
        {
            string enlace = dominio_text.Text.Trim();

            if (!string.IsNullOrEmpty(enlace))
            {
                try
                {
                    Uri uri = new Uri(enlace);
                    string dominio = uri.Host.StartsWith("www.") ? uri.Host.Substring(4) : uri.Host;
                    IPAddress[] direccionesIP = Dns.GetHostAddresses(uri.Host);
                    box_dominio.Text = dominio.ToString();
                    cuadro_ip.Text = string.Join(", ", Array.ConvertAll(direccionesIP, x => x.ToString()));
                }
                catch (UriFormatException)
                {
                    MessageBox.Show("La URL no es válida", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Ingrese un enlace válido", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void VerificarHTTP()
        {
            string dominio = box_dominio.Text.Trim();
            string protocolo = "";

            if (btn_http.Checked)
                protocolo = "http://";
            else if (btn_https.Checked)
                protocolo = "https://";

            if (!string.IsNullOrEmpty(dominio) && !string.IsNullOrEmpty(protocolo))
            {
                // Construye y muestra el enlace
                string enlace = protocolo + dominio;
                richTextBox1.SelectionColor = Color.Cyan;
                richTextBox1.AppendText("=> Enlace construido: " + enlace + Environment.NewLine);
                richTextBox1.SelectionColor = richTextBox1.ForeColor;
            }

            else
            {
                richTextBox1.SelectionColor = Color.Red;
                richTextBox1.AppendText("Fallo al generar el enlace " + Environment.NewLine);
                richTextBox1.SelectionColor = richTextBox1.ForeColor;
            }
        }

        private async void btn_validar_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            FiltrarDominio();
            VerificarHTTP();

            string rutaActual = Directory.GetCurrentDirectory();
            richTextBox1.SelectionColor = Color.GreenYellow;
            richTextBox1.AppendText(rutaActual + Environment.NewLine);
            richTextBox1.SelectionColor = richTextBox1.ForeColor;

            string ip = cuadro_ip.Text.Trim();
            if (!string.IsNullOrEmpty(ip))
            {
                string apiUrl = $"http://ip-api.com/json/{ip}";
                using (HttpClient httpClient = new HttpClient())
                {
                    try
                    {
                        string jsonResponse = await httpClient.GetStringAsync(apiUrl);
                        IpApiResponse ipApiResponse = JsonConvert.DeserializeObject<IpApiResponse>(jsonResponse);
                        if (ipApiResponse.status == "success")
                        {
                            MostrarDatos(ipApiResponse);
                        }
                        else
                        {
                            richTextBox1.SelectionColor = Color.Red;
                            richTextBox1.AppendText("NO SE PUDO VERIFICAR LA INFORMACIÓN DE LA DIRECCIÓN IP. " + Environment.NewLine);
                            richTextBox1.SelectionColor = richTextBox1.ForeColor;
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        MessageBox.Show($"Error de conexión: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Ingrese una dirección IP válida", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MostrarDatos(IpApiResponse ipApiResponse)
        {
            cuadro_ip.Text = ipApiResponse.query;
            box_country.Text = ipApiResponse.country;
            box_city.Text = ipApiResponse.region;
            box_region.Text = ipApiResponse.city;
            box_timezone.Text = ipApiResponse.timezone;
            box_isp.Text = ipApiResponse.isp;
            box_org.Text = ipApiResponse.org;
        }

        public async void ObtenerIP()
        {
            var client = new RestClient("https://api.myip.com/");
            var request = new RestRequest();
            RestResponse response = await client.ExecuteAsync(request);
            var apijson = System.Text.Json.JsonSerializer.Deserialize<StructureAPi>(response.Content);
            string ip = apijson.ip;
            string pais = apijson.country;
            string ccode = apijson.cc;
            t_ip.Text = ip.ToString();
        }

        private void btn_http_CheckedChanged(object sender, EventArgs e)
        {
            if (sender == btn_http && btn_http.Checked)
                btn_https.Checked = false;
            else if (sender == btn_https && btn_https.Checked)
                btn_http.Checked = false;
        }

        private async void btn_start_Click(object sender, EventArgs e)
        {
            if (btn_subdominios.Checked)
            {
                await Task.Run(() => CheckSubdominio());

            }
            else if (btn_directorios.Checked)
            {
                await Task.Run(() => CheckDirectorios());
            }
            else
            {
                
            }
        }

        // VERIFICAR SUBDOMINIOS

        private List<string> ObtenerSubdominios(string archivoSubdominios)
        {
            List<string> subdominios = new List<string>();

            try
            {
                using (StreamReader sr = new StreamReader(archivoSubdominios))
                {
                    string linea;
                    while ((linea = sr.ReadLine()) != null)
                    {
                        subdominios.Add(linea.Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                richTextBox1.SelectionColor = Color.Red;
                richTextBox1.AppendText("ERROR AL LEER EL ARCHIVO DE TEXTO, VERIFICA LA EXISTENCIA DEL ARCHIVO." + Environment.NewLine);
                richTextBox1.SelectionColor = richTextBox1.ForeColor;
            }

            return subdominios;
        }

        private void ActualizarTrackBar(int valor)
        {
            if (trackBar1.InvokeRequired)
            {
                trackBar1.Invoke(new Action<int>(ActualizarTrackBar), valor);
            }
            else
            {
                trackBar1.Value = valor;
            }
        }

        private void ActualizarRichTextBox(string mensaje, Color color)
        {
            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.Invoke(new Action<string, Color>(ActualizarRichTextBox), mensaje, color);
            }
            else
            {
                richTextBox1.SelectionColor = color;
                richTextBox1.AppendText(mensaje + Environment.NewLine);
                richTextBox1.SelectionColor = richTextBox1.ForeColor;
            }
        }

        private bool VerificarSubdominio(string url)
        {
            try
            {
                Uri uri = new Uri(url);
                string host = uri.Host;

                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    IAsyncResult result = socket.BeginConnect(host, 80, null, null);

                    bool success = result.AsyncWaitHandle.WaitOne(500, true);

                    if (success && socket.Connected)
                    {
                        socket.EndConnect(result);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al verificar subdominio: {ex.Message}");
                return false;
            }
        }

        private void CheckSubdominio()
        {
            string dominio = box_dominio.Text;
            string archivoSubdominios = "default.txt";
            string protocolo = "";

            List<string> subdominios = ObtenerSubdominios(archivoSubdominios);

            if (btn_http.Checked)
                protocolo = "http://";
            else if (btn_https.Checked)
                protocolo = "https://";

            if (!string.IsNullOrEmpty(dominio) && !string.IsNullOrEmpty(protocolo))
            {
                int totalSubdominios = subdominios.Count;

                Invoke(new Action(() =>
                {
                    progressBar1.Maximum = totalSubdominios;
                    progressBar1.Value = 0;
                }));

                for (int i = 0; i < totalSubdominios; i++)
                {
                    string subdominio = subdominios[i];
                    string url = $"{protocolo}{subdominio}.{dominio}";

                    bool existe = VerificarSubdominio(url);

                    Invoke(new Action(() =>
                    {
                        ActualizarRichTextBox($"{url} {(existe ? "existe" : "no existe")}", existe ? Color.Green : Color.Red);
                        progressBar1.Value = i + 1;
                    }));
                }
            }
            else
            {
                Invoke(new Action(() =>
                {
                    ActualizarRichTextBox("Fallo al generar el enlace", Color.Red);
                }));
            }
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            int valor = trackBar1.Value;
            ActualizarTrackBar(valor);
        }

        // VERIFICAR DIRECTORIOS

        private List<string> ObtenerDirectorios(string archivo)
        {
            List<string> directorios = new List<string>();

            try
            {
                string[] lineas = File.ReadAllLines(archivo);
                foreach (string linea in lineas)
                {
                    directorios.Add(linea.Trim());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al leer el archivo de directorios: {ex.Message}");
            }
            return directorios;
        }

        private int ObtenerStatusCode(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = 9000;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    return (int)response.StatusCode;
                }
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse response)
                {
                    return (int)response.StatusCode;
                }
                return -1;
            }
            catch
            {
                return -1;
            }
        }

        private void CheckDirectorios()
        {
            string dominio = box_dominio.Text;
            string archivoDirectorios = "directorios.txt";
            string protocolo = "";

            List<string> directorios = ObtenerDirectorios(archivoDirectorios);

            if (btn_http.Checked)
                protocolo = "http://";
            else if (btn_https.Checked)
                protocolo = "https://";

            if (!string.IsNullOrEmpty(dominio) && !string.IsNullOrEmpty(protocolo))
            {
                int totalDirectorios = directorios.Count;

                Invoke(new Action(() =>
                {
                    progressBar1.Maximum = totalDirectorios;
                    progressBar1.Value = 0;
                }));

                for (int i = 0; i < totalDirectorios; i++)
                {
                    string directorio = directorios[i];
                    string url = $"{protocolo}{dominio}/{directorio}";

                    int statusCode = ObtenerStatusCode(url);

                    Invoke(new Action(() =>
                    {
                        richTextBox1.SelectionColor = statusCode == 200 ? Color.Green : Color.Red;
                        richTextBox1.AppendText($"{url} {(statusCode == 200 ? "válido" : "no válido")}" + Environment.NewLine);
                        richTextBox1.SelectionColor = richTextBox1.ForeColor;

                        progressBar1.Value = i + 1;
                    }));
                }
            }
            else
            {
                Invoke(new Action(() =>
                {
                    richTextBox1.SelectionColor = Color.Red;
                    richTextBox1.AppendText("Fallo al generar el enlace " + Environment.NewLine);
                    richTextBox1.SelectionColor = richTextBox1.ForeColor;
                }));
            }
        }
        


        public class StructureAPi
        {
            public string cc { get; set; }
            public string country { get; set; }
            public string ip { get; set; }
        }


        public class IpApiResponse
        {
            public string status { get; set; }
            public string country { get; set; }
            public string countryCode { get; set; }
            public string region { get; set; }
            public string regionName { get; set; }
            public string city { get; set; }
            public string zip { get; set; }
            public float lat { get; set; }
            public float lon { get; set; }
            public string timezone { get; set; }
            public string isp { get; set; }
            public string org { get; set; }
            public string _as { get; set; }
            public string query { get; set; }
        }

    }
}
