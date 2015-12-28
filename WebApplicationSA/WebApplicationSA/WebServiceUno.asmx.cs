using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data;
using System.Data.SqlClient;

namespace WebApplicationSA
{
    /// <summary>
    /// Summary description for WebServiceUno
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class WebServiceUno : System.Web.Services.WebService
    {
        /***    Roger  ****/
        //static private String ipp = "192.168.1.6";
        //tatic private String connection = "Server=" + ipp + ";Database=miDB;User Id=externo;Password = RgM712712712; ";
         
        /*** Rodrigo ***/
        static private String ipp = "192.168.40.129";
        static private String connection = "Server=" + ipp + ";Database=BDFarmacia;User Id=WSSA;Password = 12345; ";

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }
        [WebMethod]
        public DataSet getClientes(int IDUsuario)
        {
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = "Server="+ipp+";Database=miDB;User Id=externo;Password = RgM712712712; ";
            SqlDataAdapter sda = new SqlDataAdapter("SELECT * FROM cliente", conn);
            DataSet ds = new DataSet();
            sda.Fill(ds);

            /*DateTime myDateTime = DateTime.Now;
            string sqlFormattedDate = myDateTime.ToString("yyyy-MM-dd HH:mm:ss");

            SqlCommand cmd = new System.Data.SqlClient.SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText =   "INSERT INTO Bitacora ([FechaHora] ,[TipoTransaccion] ,[Detalle] ,[IDUsuario]) VALUES  ( '"+
                sqlFormattedDate + "' , " + "'getClientes'"
                + " , " + "'se consulta get clientes'"
                   +" , " + IDUsuario+ ")";
            cmd.Connection = conn; 
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
            */

            return ds;
        }
        
        [WebMethod]
        public Respuesta setCliente_WS02(String nit, string nombre, int telefono  , string direccion, int IDUsuario)
        {
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = "Server=" + ipp + ";Database=miDB;User Id=externo;Password = RgM712712712; ";
            conn.Open();


            // 1.  identificar el SP
            SqlCommand cmd = new SqlCommand("WS02", conn); // asi se llama el procedimeinto almacenado

            // 2. set the command object so it knows to execute a stored procedure
            cmd.CommandType = CommandType.StoredProcedure;
              
            // 3. agregar los parametros
            cmd.Parameters.Add(new SqlParameter("@NIT", nit));
            cmd.Parameters.Add(new SqlParameter("@Nombre", nombre));
            cmd.Parameters.Add(new SqlParameter("@Telefono", telefono));
            cmd.Parameters.Add(new SqlParameter("@Direccion", direccion)); 
            cmd.Parameters.Add(new SqlParameter("@IDUsuario", IDUsuario));

            Respuesta res = new Respuesta();
            res.listaRes = new List<object>();
            res.listaRes.Add(-10);
            res.listaRes.Add("naranjas");
            res.listaRes.Add(IDUsuario);
            using (SqlDataReader rdr = cmd.ExecuteReader())
            {
                // iterar en los resultados
                while (rdr.Read())
                {
                    //esto es pa que borre lod e antes
                    res.listaRes = new List<object>();
                    // de una vez pongo el return ... obvio para mas de un resultado no lo deberia de tener
                    res.listaRes.Add(0);
                    res.listaRes.Add("Exito insertando");
                    res.listaRes.Add(rdr[0]);
                    return res;
                }
            }

            return res; // por gusto esta este
        }


        [WebMethod]
        public DataSet WS00a_RegistrarCompra(int IDFarmacia, String tipoPago, string direccionEntrega, int IDUsuario, int IDCliente)
        {
            Respuesta res = new Respuesta();

            SqlConnection conn = new SqlConnection();
            SqlCommand cmd = new SqlCommand();
            conn.ConnectionString = connection;
            cmd.CommandText = "INSERT INTO COMPRA ([Fecha], [Destino],[Estado],[TipoPago],[Total],[IDUsuario],[IDFarmacia],[IDCliente]) "+
            "VALUES (@fecha, @Destino, @Estado, @TipoPago, @Total, @IDUsuario, @IDFarmacia, @IDCliente);";

            cmd.Parameters.AddWithValue("@fecha", DateTime.Now);
            cmd.Parameters.AddWithValue("@destino", direccionEntrega);
            cmd.Parameters.AddWithValue("@Estado", "pediente");
            cmd.Parameters.AddWithValue("@TipoPago", tipoPago);
            cmd.Parameters.AddWithValue("@Total", 0.00);
            cmd.Parameters.AddWithValue("@IDUsuario", IDUsuario);
            cmd.Parameters.AddWithValue("@IDFarmacia", IDFarmacia);
            cmd.Parameters.AddWithValue("@IDCliente", IDCliente);

            cmd.Connection = conn;

            conn.Open();
            cmd.ExecuteNonQuery();

            SqlDataAdapter sda = new SqlDataAdapter("SELECT TOP 1 [IDCOMPRA],[FECHA],[DESTINO],[ESTADO],[TIPOPAGO],[TOTAL],[IDUSUARIO],[IDFARMACIA],[IDCLIENTE] FROM COMPRA ORDER BY IDCOMPRA DESC", conn);
            DataSet ds = new DataSet();
            sda.Fill(ds);


            cmd.CommandText = "INSERT INTO Bitacora ([Fecha] ,[TipoTransaccion] ,[Detalle] ,[IDUsuario]) VALUES  (@fecha, @TipoTransaccion, @Detalle, @IDUsuario )";
            cmd.Parameters.AddWithValue("@fecha", DateTime.Now);
            cmd.Parameters.AddWithValue("@TipoTransaccion", "Registrar Compra");
            cmd.Parameters.AddWithValue("@Detalle", "Se ha registrado una nueva compra con éxito");
            cmd.Parameters.AddWithValue("@IDUsuario", IDUsuario);
            cmd.ExecuteNonQuery();


            conn.Close();
            return ds;
        }

        [WebMethod]
        public Respuesta WS00b_AgregarDetalle(int IDCompra, int IDMedicamento, int cantidad, int IDFarmacia)
        {
            Respuesta res = new Respuesta();
            Double precioUnitario = 0.00;
            Double totalCompra = 0;
            int idInventario = -1;
            SqlConnection conn = new SqlConnection();
            SqlCommand cmd = new SqlCommand();
            conn.ConnectionString = connection;
            cmd.CommandText = "SELECT [PRECIO] FROM MEDICAMENTO WHERE [IDMEDICAMENTO] = @IDMedicamento;";
            cmd.Parameters.AddWithValue("@IDMedicamento", IDMedicamento);
            cmd.Connection = conn;
            conn.Open();
            SqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {

                precioUnitario = Double.Parse(((decimal)rdr["PRECIO"]).ToString()) ;
            }

            rdr.Close();
            
            cmd.CommandText = "SELECT [IDINVENTARIO] FROM INVENTARIO WHERE [IDMEDICAMENTO] = @IDMedicamento AND [IDFARMACIA] = @IDFarmacia;";
            cmd.Parameters.AddWithValue("@IDFarmacia", IDFarmacia);

            rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                idInventario = (int) rdr["IDINVENTARIO"];
            }
            rdr.Close();


            cmd.CommandText = "INSERT INTO DETALLECOMPRA ([CANTIDAD],[IDCOMPRA],[IDINVENTARIO]) VALUES (@cantidad, @idcompra, @idinventario);";
            cmd.Parameters.AddWithValue("@cantidad", cantidad);
            cmd.Parameters.AddWithValue("@idcompra", IDCompra);
            cmd.Parameters.AddWithValue("@idinventario", idInventario);

            cmd.ExecuteNonQuery();

            cmd.CommandText = "SELECT [TOTAL] FROM COMPRA WHERE [IDCOMPRA] = @idcompra";
            rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                totalCompra = Double.Parse(((decimal)rdr["TOTAL"]).ToString());
            }

            rdr.Close();

            totalCompra = totalCompra + (cantidad * precioUnitario);

            cmd.CommandText = "UPDATE COMPRA SET [TOTAL] = @totalCompra WHERE [IDCOMPRA] = @idcompra;";
            cmd.Parameters.AddWithValue("@totalCompra", totalCompra);
            cmd.ExecuteNonQuery();


            conn.Close();

            res.listaRes.Add("Se agregaron medicamentos a la compra: " + IDCompra + " exitosamente");

            return res;
        }

        [WebMethod]
        public Respuesta WS01_RegistrarPago(int IDCompra, int IDUsuario)
        {
            Respuesta res = new Respuesta();

            SqlConnection conn = new SqlConnection();
            SqlCommand cmd = new SqlCommand();
            Boolean exito = true;

            conn.ConnectionString = connection;
            cmd.Connection = conn;
            conn.Open();
            try
            {
                cmd.CommandText = "UPDATE COMPRA SET [ESTADO] = @estado WHERE [IDCOMPRA]  = @idcompra;";
                cmd.Parameters.AddWithValue("@estado", "pagado");
                cmd.Parameters.AddWithValue("@idcompra", IDCompra);
                cmd.ExecuteNonQuery();
            }
            catch
            {
                exito = false;
            }

            if (exito)
            {
                cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "INSERT INTO Bitacora ([Fecha] ,[TipoTransaccion] ,[Detalle] ,[IDUsuario]) VALUES  (@fecha, @TipoTransaccion, @Detalle, @IDUsuario )";
                cmd.Parameters.AddWithValue("@fecha", DateTime.Now);
                cmd.Parameters.AddWithValue("@TipoTransaccion", "Registrar Pago");
                cmd.Parameters.AddWithValue("@Detalle", "Se ha registrado el pago de la compra: " + IDCompra);
                cmd.Parameters.AddWithValue("@IDUsuario", IDUsuario);
                cmd.ExecuteNonQuery();

                res.listaRes.Add(0);
                res.listaRes.Add("El pago se ha registrado exitosamente");
                res.listaRes.Add(IDCompra);
            }
            else
            {
                res.listaRes.Add(-1);
                res.listaRes.Add("El pago no se ha registrado");
                res.listaRes.Add(IDCompra);
            }


            conn.Close();



            return res;
        }


        [WebMethod]
        public Respuesta WS02_AlmacenarCliente(String NIT, String nombre, String telefono, String direccion, String IDUsuario)
        {
            Respuesta res = new Respuesta();

            SqlConnection conn = new SqlConnection();
            SqlCommand cmd = new SqlCommand();
            bool exito = true;

            conn.ConnectionString = connection;
            cmd.Connection = conn;
            
            cmd.CommandText = "INSERT INTO CLIENTE ([NOMBRE], [NIT], [DIRECCION], [TELEFONO]) VALUES (@nombre, @nit, @direccion, @telefono)";
            cmd.Parameters.Add(new SqlParameter("@nombre", nombre));
            cmd.Parameters.Add(new SqlParameter("@nit", NIT));
            cmd.Parameters.Add(new SqlParameter("@direccion", direccion));
            cmd.Parameters.Add(new SqlParameter("@telefono", telefono));



            conn.Open();
            res.listaRes = new List<object>();

            try
            {
                cmd.ExecuteNonQuery();
                res.listaRes.Add(0);
                res.listaRes.Add("Cliente agregado exitosamente");
            }
            catch (Exception e)
            {
                res.listaRes.Add(-1);
                res.listaRes.Add(e.ToString());
                exito = false;
            }


            if (exito)
            {
                cmd = new System.Data.SqlClient.SqlCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "INSERT INTO Bitacora ([Fecha] ,[TipoTransaccion] ,[Detalle] ,[IDUsuario]) VALUES  (@fecha, @TipoTransaccion, @Detalle, @IDUsuario )";
                cmd.Parameters.AddWithValue("@fecha", DateTime.Now);
                cmd.Parameters.AddWithValue("@TipoTransaccion", "Agregar Nuevo Cliente");
                cmd.Parameters.AddWithValue("@Detalle", "Se agregró al cliente " + nombre + " con NIT: " + NIT);
                cmd.Parameters.AddWithValue("@IDUsuario", IDUsuario);
                cmd.Connection = conn;
                cmd.ExecuteNonQuery();
            }
                       
            
            conn.Close();

            return res;
        }

        [WebMethod]
        public Respuesta WS03_getCliente(String nit, int IDUsuario)
        {
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = connection;
            conn.Open();


            // 1.  identificar el SP
            SqlCommand cmd = new SqlCommand("WS03", conn); // asi se llama el procedimeinto almacenado

            // 2. set the command object so it knows to execute a stored procedure
            cmd.CommandType = CommandType.StoredProcedure;

            // 3. agregar los parametros
            cmd.Parameters.Add(new SqlParameter("@NIT", nit));
            cmd.Parameters.Add(new SqlParameter("@IDUsuario", IDUsuario));

            Respuesta res = new Respuesta();
            res.listaRes = new List<object>();
            res.listaRes.Add(-1);
            res.listaRes.Add("no hay nada");
            res.listaRes.Add(-1);
            res.listaRes.Add("");
            res.listaRes.Add("");
            res.listaRes.Add("");
            using (SqlDataReader rdr = cmd.ExecuteReader())
            {
                // iterar en los resultados
                while (rdr.Read())
                {
                    //esto es pa que borre lod e antes
                    res.listaRes = new List<object>();
                    // de una vez pongo el return ... obvio para mas de un resultado no lo deberia de tener
                    res.listaRes.Add(0);
                    res.listaRes.Add("Exito");
                    res.listaRes.Add(rdr["IDCliente"]);
                    res.listaRes.Add(rdr["Nombre"]);
                    res.listaRes.Add(rdr["Direccion"]);
                    res.listaRes.Add(rdr["Telefono"]);
                    return res;
                }
            }

            return res; // por gusto esta este
        }
        
         [WebMethod]
        public List<Respuesta> getMedicamento_WS04(int IDFarmacia, int IDMedicamento, string Nombre , int IDUsuario)
        {
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = "Server=" + ipp + ";Database=miDB;User Id=externo;Password = RgM712712712; ";
            conn.Open();


            // 1.  identificar el SP
            SqlCommand cmd = new SqlCommand("WS04", conn); // asi se llama el procedimeinto almacenado

            // 2. set the command object so it knows to execute a stored procedure
            cmd.CommandType = CommandType.StoredProcedure;

            // 3. agregar los parametros
            cmd.Parameters.Add(new SqlParameter("@IDFarmacia", IDFarmacia));
            cmd.Parameters.Add(new SqlParameter("@IDMedicamento", IDMedicamento));
            cmd.Parameters.Add(new SqlParameter("@Nombre", Nombre));
            cmd.Parameters.Add(new SqlParameter("@IDUsuario", IDUsuario));

            List <Respuesta> lres = new List<Respuesta>();
            using (SqlDataReader rdr = cmd.ExecuteReader())
            {
                // iterar en los resultados
                while (rdr.Read())
                {
                    Respuesta res = new Respuesta();
                    res.listaRes = new List<object>();
                    res.listaRes = new List<object>();
                    res.listaRes.Add(rdr[0]);
                    res.listaRes.Add(rdr[1]);
                    res.listaRes.Add(rdr[2]);
                    res.listaRes.Add(rdr[3]);
                    res.listaRes.Add(rdr[4]);
                    lres.Add( res);
                }
            }

            return lres; // por gusto esta este
        }
        
        [WebMethod]
        public int getIngresar_WS(String Nombre, String Password)
        {
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = "Server=" + ipp + ";Database=miDB;User Id=externo;Password = RgM712712712; ";
            conn.Open();


            // 1.  identificar el SP
            SqlCommand cmd = new SqlCommand("Ingresar", conn); // asi se llama el procedimeinto almacenado

            // 2. set the command object so it knows to execute a stored procedure
            cmd.CommandType = CommandType.StoredProcedure;
            
            // 3. agregar los parametros
            cmd.Parameters.Add(new SqlParameter("@Nombre", Nombre));
            cmd.Parameters.Add(new SqlParameter("@Password", Password));

            int res = -1;
            using (SqlDataReader rdr = cmd.ExecuteReader())
            {
                // iterar en los resultados
                while (rdr.Read())
                {
                    //esto es pa que borre lod e antes
                    res = (int)rdr[0];
                    
                    return res;
                }
            }

            return res; // por gusto esta este
        }

        [WebMethod]
        public Respuesta WS09_ValidarLogin(string password, string username)
        {
            Respuesta res = new Respuesta();
            SqlCommand cmd = new SqlCommand();
            SqlConnection conn = new SqlConnection();
            int IDUsuario = -1;
            conn.ConnectionString = connection;
            cmd.Connection = conn;

            conn.Open();

            cmd.CommandText = "SELECT * FROM USUARIO;";

            SqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                String nombre = (String)rdr["NOMBRE"];
                String pass = (String)rdr["PASSWORD"];

                if (nombre == username && pass == password)
                {
                    IDUsuario = (int)rdr["IDUSUARIO"];

                    res.listaRes.Add("Ha iniciado sesión exitosamente");
                    res.listaRes.Add(rdr["IDUSUARIO"]);
                    res.listaRes.Add(rdr["NOMBRE"]);
                    res.listaRes.Add(rdr["PASSWORD"]);
                    res.listaRes.Add(rdr["ROL"]);

                    cmd.CommandText = "INSERT INTO Bitacora ([Fecha] ,[TipoTransaccion] ,[Detalle] ,[IDUsuario]) VALUES  (@fecha, @TipoTransaccion, @Detalle, @IDUsuario )";
                    cmd.Parameters.AddWithValue("@fecha", DateTime.Now);
                    cmd.Parameters.AddWithValue("@TipoTransaccion", "Iniciar Sesión");
                    cmd.Parameters.AddWithValue("@Detalle", "El usuario "+IDUsuario+" ha iniciado sesión");
                    cmd.Parameters.AddWithValue("@IDUsuario", IDUsuario);
                    rdr.Close();
                    cmd.ExecuteNonQuery();

                    return res;
                }
            }

            res.listaRes.Add("Error al iniciar sesión, verificar datos.");
            conn.Close();
            return res;
        }

        [WebMethod]
        public Respuesta WS10_CrearUsuario(int IDUsuario, string username, string password, string rol)
        {
            Respuesta res = new Respuesta();

            SqlConnection conn = new SqlConnection();
            SqlCommand cmd = new SqlCommand();
            conn.ConnectionString = connection;
            cmd.Connection = conn;

            conn.Open();

            cmd.CommandText = "INSERT INTO USUARIO ([NOMBRE],[PASSWORD],[ROL]) VALUES (@username, @password, @rol);";
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", password);
            cmd.Parameters.AddWithValue("@rol", rol);

            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT INTO Bitacora ([Fecha] ,[TipoTransaccion] ,[Detalle] ,[IDUsuario]) VALUES  (@fecha, @TipoTransaccion, @Detalle, @IDUsuario )";
            cmd.Parameters.AddWithValue("@fecha", DateTime.Now);
            cmd.Parameters.AddWithValue("@TipoTransaccion", "Crear Usuario");
            cmd.Parameters.AddWithValue("@Detalle", "Se ha creado al usuario: "+username);
            cmd.Parameters.AddWithValue("@IDUsuario", IDUsuario);

            cmd.ExecuteNonQuery();

            conn.Close();

            res.listaRes.Add("Se ha creado al usuario: " + username + " exitósamente");

            return res;
        }





    }


    
    [Serializable]
    public class Respuesta
    {
        /*public int codigoRespuesta { get; set; } //(0= éxito, -1 = no se ingresó, …,  -10=  error desconocido) cod_respuesta
        public int idCliente { get; set; } // -1 no existe
        public string nombre { get; set; }
        public string direccion { get; set; }
        public string telefono { get; set; }*/
        public List<Object> listaRes { get; set; }

        public Respuesta()
        {
            this.listaRes = new List<object>();
        }
    }
}
