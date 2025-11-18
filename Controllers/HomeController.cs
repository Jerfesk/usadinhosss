using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using usadinhosss.Models;
using System.Data.SQLite;


namespace usadinhosss.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult adm()
        {
            return View();
        }

        public IActionResult estoque()
        {
            return View();
        }

        public IActionResult contato()
        {
            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult cadastro()
        {
            return View();
        }

        public IActionResult BuscarVeiculo(String modelo) //função p/ buscar veículos no BD, por modelo ou todos se o modelo for vazio
        {
            String select = "";                        //variável p/ armazenar o comando SQL de seleção dos dados no BD
            if (modelo == null || modelo.Equals(""))  //se o modelo for vazio, busca todos os veículos
            {
                select = "select * from veiculos";    //comando SQL p/ buscar todos os veículos no BD
            }
            else                                      //modelo não for vazio, busca os veículos que contenham o modelo informado
            {
                select = "select * from veiculos where modelo like '%" + modelo + "%'";  //comando SQL p/ buscar os veículos que contenham o modelo informado no BD
            }

            String stringConnection = "Data Source=usadinhosss.bd.db; Version = 3; New = True; Compress = True; ";  //string de conexão com o BD, usadinhosss.bd.db nome do arquivo do BD, esta na pasta do projeto.
            SQLiteConnection sqlite_conn = new SQLiteConnection(stringConnection);    //cria a conexão com o BD
            sqlite_conn.Open();                                                       //abre a conexão com o BD, sempre em conexões se abre e fecha
            SQLiteCommand comandoSQL = new SQLiteCommand(select, sqlite_conn);       //cria o comando SQL para ser executado no BD
            SQLiteDataReader dr = comandoSQL.ExecuteReader();                       //executa o comando SQL, retorna os dados selecionados do BD
            List<Veiculo> listaVeiculos = new List<Veiculo>();                      //lista p/ armazenar os veículos retornados do BD

            while (dr.Read())                                //lê os dados retornados do BD, enquanto houver dados
            { 
                Veiculo v = new Veiculo();                   //cria um objeto Veiculo p/ armazenar os dados do veículo
                v.marca = dr["marca"].ToString();           //armazenar os dados do veículo retornados do BD
                v.modelo = dr["modelo"].ToString();         //armazenar os dados do veículo retornados do BD
                v.ano = dr["ano"].ToString();               //armazenar os dados do veículo retornados do BD
                v.cor = dr["cor"].ToString();              //armazenar os dados do veículo retornados do BD
                v.preco = dr["preco"].ToString();          //armazenar os dados do veículo retornados do BD
                listaVeiculos.Add(v);                      //adiciona o veículo na lista de veículos

            }

            return Json(listaVeiculos);                  //retorna a lista de veículos em formato JSON
        }

        [HttpPost]    // tipo POST é mais seguro do que o metodo GET
        public IActionResult inserirVeiculo([FromBody] Veiculo veiculos)  //função p/ inserir daddos no BD, 
        {                                                                 // Veiculo é a classe criada abaixo e veiculos é o nome da tabela no BD.
                                                                          // [FromBody] indica que os dados virão do corpo da requisição HTTP.
            String cmdinsert = "insert into veiculos(marca,modelo,ano,cor,preco) ";  //comando SQL p/ inserir dados na tabela veiculos
            cmdinsert += $"values ('{veiculos.marca}','{veiculos.modelo}','{veiculos.ano}','{veiculos.cor}','{veiculos.preco}')";  //valores a serem inseridos


            String stringConnection = "Data Source=usadinhosss.bd.db; Version = 3; New = True; Compress = True; ";  //string de conexão com o BD, usadinhosss.bd.db nome do arquivo do BD, esta na pasta do projeto.
            SQLiteConnection sqlite_conn = new SQLiteConnection(stringConnection);  //cria a conexão com o BD
            sqlite_conn.Open();                                                     //abre a conexão com o BD, sempre em conexões se abre e fecha

            SQLiteCommand comandoSQL = new SQLiteCommand(cmdinsert, sqlite_conn);  //cria o comando SQL para ser executado no BD
            int qtd_linhas_inseridas = comandoSQL.ExecuteNonQuery();               //executa o comando SQL, retorna a quantidade de linhas inseridas no BD

            string resposta = "";           //variável p/ armazenar a resposta da operação de inserção
            if (qtd_linhas_inseridas > 0)  //se a quant de linhas inseridas for maior que 0, ou seja, se a inserção foi bem sucedida
                resposta = "Dados inseridos com sucesso!!!";
            else
                resposta = "Não foi possível inserir!!!";

            sqlite_conn.Close();         //fecha a conexão com o BD, como foi aberta anteriormente, sempre se fecha

            return Json(resposta);       //retorna a resposta da operação de inserção em formato JSON

        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]  //indica que a resposta não deve ser armazenada em cache
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult Autenticar([FromBody] LoginRequest dados)
        {
            // A string de conexão deve ser a que criamos para tabela de adm
            string connectionString = "Data Source=adm_usadinho.db";

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Consulta para verificar se existe um registro com o login e senha fornecidos
                // O $ antes da string é para usar interpolação (variáveis direto na string)
                string query = $"SELECT COUNT(*) FROM Adm WHERE login = @login AND senha = @senha";

                using (var command = new SQLiteCommand(query, connection))
                {
                    // Adiciona parâmetros para evitar SQL Injection (boa prática!)
                    command.Parameters.AddWithValue("@login", dados.login);
                    command.Parameters.AddWithValue("@senha", dados.senha);

                    // Executa a consulta e pega o resultado (que será 1 se o usuário existir, ou 0 se não)
                    long count = (long)command.ExecuteScalar();

                    if (count > 0)
                    {
                        // Login OK: Retorna um status 200 (OK) com uma mensagem de sucesso
                        return Ok(new { sucesso = true, mensagem = "Login efetuado com sucesso!" });
                    }
                    else
                    {
                        // Login Falhou: Retorna um status 401 (Não Autorizado) com a mensagem de erro
                        // É melhor retornar 401 do que 200 com erro.
                        return Unauthorized(new { sucesso = false, mensagem = "Login ou senha incorretos." });
                    }
                }
            }
        }

        public class Veiculo     //classe p/ representar os dados do veículo a serem inseridos no BD, criado na função inserirVeiculo
        {
            public String marca { get; set; }  //abaixo são os dados p/ armazenar no BD na tabela veiculos
            public String modelo { get; set; }
            public String ano { get; set; }
            public String cor { get; set; }
            public String preco { get; set; }
        }

        public class LoginRequest    // classe p/ requisitar os dados de login, criado na função Login
        {
            public string login { get; set; }
            public string senha { get; set; }
        }
    }
}
