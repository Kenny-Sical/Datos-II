﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Linq;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        // Crear una instancia del procesador AVL
        var processor = new AVLProcessor();

        // Procesar el archivo de bitácora
        processor.ProcessFile(@"C:\Users\sical\OneDrive\Escritorio\inputs\input (2).csv");
        MessageProcessor Cipher = new MessageProcessor("MILLAVE");
        //Busqueda
        bool salir = true;
        while (salir)
        {
            Console.WriteLine("Introduce un DPI para buscar:");
            string dpiToSearch = Console.ReadLine();

            // Codifica el DPI
            var encodedDpiToSearchList = processor.Encode(dpiToSearch);
            string encodedDpiToSearch = JsonConvert.SerializeObject(encodedDpiToSearchList);
            var result = processor.SearchByDPI(encodedDpiToSearch);
            //Mostrar Codificado o Sin codificar
            Console.WriteLine("¿Mostrar Codificado Si/No?");
            string mostrar = Console.ReadLine();
            if (mostrar.ToUpper() == "NO")
            {
                if (result != null)
                {
                    // Decodifica el DPI
                    var decodedDpiList = JsonConvert.DeserializeObject<List<Tuple<int, char>>>(result.Dpi);
                    string decodedDpi = Decode(decodedDpiList);

                    // Decodifica las compañías
                    var decodedCompaniesLists = result.Companies.Select(js => JsonConvert.DeserializeObject<List<Tuple<int, char>>>(js)).ToList();
                    var decodedCompanies = decodedCompaniesLists.Select(decodedCompanyList => Decode(decodedCompanyList)).ToList();

                    // Crea el objeto de salida
                    var output = new OutputFormatDecode
                    {
                        Name = result.Name,
                        Dpi = decodedDpi,
                        DateBirth = result.DateBirth,
                        Address = result.Address,
                        Companies = decodedCompanies
                    };

                    // Convierte el objeto de salida a JSON y lo imprime
                    string jsonString = JsonConvert.SerializeObject(output, Formatting.Indented);
                    Console.WriteLine(jsonString);
                    Archivos(decodedDpi);
                }
                else
                {
                    Console.WriteLine($"No se encontró un registro con el DPI: {dpiToSearch}");
                }
            }
            else
            {
                if (result != null)
                {
                    // Transforma la lista de tuplas del DPI a una lista de cadenas con el formato deseado
                    var transformedDpiList = JsonConvert.DeserializeObject<List<Tuple<int, char>>>(result.Dpi).Select(tuple => $"({tuple.Item1},{tuple.Item2})").ToList();

                    // Transforma cada compañía
                    var transformedCompaniesList = result.Companies
                        .Select(companyJson =>
                        {
                            var tuples = JsonConvert.DeserializeObject<List<Tuple<int, char>>>(companyJson);
                            return tuples.Select(tuple => $"({tuple.Item1},{tuple.Item2})").ToList();
                        }).ToList();

                    // Crea el objeto de salida con los datos originales
                    var output = new OutputFormatEncode
                    {
                        Name = result.Name,
                        Dpi = transformedDpiList,
                        DateBirth = result.DateBirth,
                        Address = result.Address,
                        Companies = transformedCompaniesList
                    };

                    // Convierte el objeto de salida a JSON y lo imprime
                    string jsonString = JsonConvert.SerializeObject(output, Formatting.Indented);
                    Console.WriteLine(jsonString);

                    //Mandar a buscar las cartas recomendadas
                    // Decodifica el DPI
                    var decodedDpiList = JsonConvert.DeserializeObject<List<Tuple<int, char>>>(result.Dpi);
                    string decodedDpi = Decode(decodedDpiList);
                    Archivos(decodedDpi);
                }
                else
                {
                    Console.WriteLine($"No se encontró un registro con el DPI: {dpiToSearch}");
                }
            }
            Console.WriteLine("¿Decea salir?");
            string decision = Console.ReadLine();
            if (decision.ToUpper() == "SI")
            {
                salir = false;
            }
            else
            {
                Console.Clear();
            }
            Console.ReadKey();
        }
        string Decode(List<Tuple<int, char>> encoded)
        {
            List<string> dictionary = new List<string> { "" };
            StringBuilder decoded = new StringBuilder();

            foreach (var tuple in encoded)
            {
                string entry;
                if (tuple.Item1 == 0)
                {
                    entry = tuple.Item2.ToString();
                }
                else
                {
                    entry = dictionary[tuple.Item1] + tuple.Item2;
                }
                decoded.Append(entry);
                dictionary.Add(entry);
            }

            return decoded.ToString();
        }
        void Archivos(string dpi)
        {
            string inputDirectoryPath = @"C:\Users\sical\OneDrive\Escritorio\inputs\inputs";
            string outputDirectoryPath = @"C:\Users\sical\OneDrive\Escritorio\inputs\inputs-Nuevos";
            var matchingFiles = Directory.GetFiles(inputDirectoryPath, $"REC-{dpi}-*").ToList();

            if (!matchingFiles.Any())
            {
                Console.WriteLine("No se encontraron archivos que coincidan con el DPI ingresado.");
                return;
            }

            foreach (var filePath in matchingFiles)
            {
                string fileContent = File.ReadAllText(filePath);
                string encrypted = Cipher.Encrypt(fileContent);
                string decrypted = Cipher.Decrypt(encrypted);

                File.WriteAllText(Path.Combine(outputDirectoryPath, Path.GetFileName(filePath) + ".encrypted"), encrypted);
                File.WriteAllText(Path.Combine(outputDirectoryPath, Path.GetFileName(filePath) + ".decrypted"), decrypted);

                Console.WriteLine($"Archivo {filePath} procesado y cifrado/descifrado correctamente.");
            }
        }
    }
}
#region AVLProcessor
public class AVLProcessor
{
    private AVLTree avlTree;

    public AVLProcessor()
    {
        avlTree = new AVLTree();
    }

    public void ProcessFile(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);
        foreach (var line in lines)
        {
            string[] parts = line.Split(';');
            if (parts.Length != 2) continue; 

            string operation = parts[0];
            Person person = JsonConvert.DeserializeObject<Person>(parts[1]);

            List<Tuple<int, char>> EncodedDpi;
            List<List<Tuple<int, char>>> EncodedCompanies;

            // Codify the DPI and convert it to a JSON string
            EncodedDpi = Encode(person.Dpi);
            person.Dpi = JsonConvert.SerializeObject(EncodedDpi);

            // Codify each company, convert the encoded list to a JSON string, and replace the original list
            EncodedCompanies = person.Companies.Select(company => Encode(company)).ToList();
            person.Companies = EncodedCompanies.Select(encodedCompany => JsonConvert.SerializeObject(encodedCompany)).ToList();

            switch (operation)
            {
                case "INSERT":
                    avlTree.Insert(person);
                    break;
                case "PATCH":
                    avlTree.Update(person.Name, person.Dpi, person);
                    break;
                case "DELETE":
                    avlTree.Delete(person.Name, person.Dpi);
                    break;
            }
        }
    }
    public Person SearchByDPI(string dpi)
    {
        return avlTree.SearchByDPI(dpi);
    }
    public List<Tuple<int, char>> Encode(string input)
    {
        List<Tuple<int, char>> encoded = new List<Tuple<int, char>>();
        Dictionary<string, int> dictionary = new Dictionary<string, int>();
        int dictSize = 1;

        string currentString = string.Empty;
        foreach (char c in input)
        {
            string combined = currentString + c;
            if (!dictionary.ContainsKey(combined))
            {
                if (dictionary.ContainsKey(currentString))
                {
                    encoded.Add(new Tuple<int, char>(dictionary[currentString], c));
                }
                else
                {
                    encoded.Add(new Tuple<int, char>(0, c));
                }

                dictionary[combined] = dictSize++;
                currentString = string.Empty;
            }
            else
            {
                currentString = combined;
            }
        }

        if (!string.IsNullOrEmpty(currentString))
        {
            if (dictionary.ContainsKey(currentString))
            {
                encoded.Add(new Tuple<int, char>(dictionary[currentString], ' '));
            }
            else
            {
                encoded.Add(new Tuple<int, char>(0, currentString[0]));
            }
        }

        return encoded;
    }
}
#endregion

#region PersonClass
public class Person
{
    public string Name { get; set; }
    public string Dpi { get; set; }
    public string DateBirth { get; set; }
    public string Address { get; set; }
    public List<string> Companies { get; set; }

    // Constructor
    public Person(string name, string dpi, string dateBirth, string address, List<string> companies)
    {
        Name = name;
        Dpi = dpi;
        DateBirth = dateBirth;
        Address = address;
        Companies = companies;
    }
}
public class OutputFormatDecode
{
    public string Name { get; set; }
    public string Dpi { get; set; }
    public string DateBirth { get; set; }
    public string Address { get; set; }
    public List<string> Companies { get; set; }
}
public class OutputFormatEncode
{
    public string Name { get; set; }
    public List<string> Dpi { get; set; }
    public string DateBirth { get; set; }
    public string Address { get; set; }
    public List<List<string>> Companies { get; set; }
}
#endregion

#region Arból
public class Node
{
    public Person Value { get; set; }
    public Node Left { get; set; }
    public Node Right { get; set; }
    public int Height { get; set; }

    // Constructor
    public Node(Person value)
    {
        Value = value;
        Left = null;
        Right = null;
        Height = 1;  // Al crear un nuevo nodo, su altura es 1
    }
}
public class AVLTree
{
    private Node root;

    public AVLTree()
    {
        root = null;
    }

    private int GetHeight(Node node)
    {
        if (node == null)
            return 0;
        return node.Height;
    }

    private int GetBalanceFactor(Node node)
    {
        if (node == null)
            return 0;
        return GetHeight(node.Left) - GetHeight(node.Right);
    }
    private Node RotateRight(Node y)
    {
        Node x = y.Left;
        Node T3 = x.Right;

        // Realizar rotación
        x.Right = y;
        y.Left = T3;

        // Actualizar alturas
        y.Height = Math.Max(GetHeight(y.Left), GetHeight(y.Right)) + 1;
        x.Height = Math.Max(GetHeight(x.Left), GetHeight(x.Right)) + 1;

        return x;  // nuevo nodo raíz
    }

    private Node RotateLeft(Node x)
    {
        Node y = x.Right;
        Node T2 = y.Left;

        // Realizar rotación
        y.Left = x;
        x.Right = T2;

        // Actualizar alturas
        x.Height = Math.Max(GetHeight(x.Left), GetHeight(x.Right)) + 1;
        y.Height = Math.Max(GetHeight(y.Left), GetHeight(y.Right)) + 1;

        return y;  // nuevo nodo raíz
    }

    private Node RotateLeftRight(Node node)
    {
        node.Left = RotateLeft(node.Left);
        return RotateRight(node);
    }

    private Node RotateRightLeft(Node node)
    {
        node.Right = RotateRight(node.Right);
        return RotateLeft(node);
    }
    public void Insert(Person person)
    {
        root = InsertRec(root, person);
    }

    private Node InsertRec(Node node, Person person)
    {
        // 1. Realizar inserción normal de BST
        if (node == null)
            return new Node(person);

        // Ordenamos los nodos por el nombre
        if (string.Compare(person.Name, node.Value.Name) < 0)
            node.Left = InsertRec(node.Left, person);
        else if (string.Compare(person.Name, node.Value.Name) > 0)
            node.Right = InsertRec(node.Right, person);
        else  // Los nombres duplicados no están permitidos en el BST
            return node;

        // 2. Actualizar altura del nodo actual
        node.Height = 1 + Math.Max(GetHeight(node.Left), GetHeight(node.Right));

        // 3. Obtener el factor de equilibrio para ver si se desequilibró
        int balance = GetBalanceFactor(node);

        // Si el nodo se desequilibra, hay 4 casos a considerar

        // Caso LL
        if (balance > 1 && string.Compare(person.Name, node.Left.Value.Name) < 0)
            return RotateRight(node);

        // Caso RR
        if (balance < -1 && string.Compare(person.Name, node.Right.Value.Name) > 0)
            return RotateLeft(node);

        // Caso LR
        if (balance > 1 && string.Compare(person.Name, node.Left.Value.Name) > 0)
            return RotateLeftRight(node);

        // Caso RL
        if (balance < -1 && string.Compare(person.Name, node.Right.Value.Name) < 0)
            return RotateRightLeft(node);

        return node;  // retornar el nodo sin cambios
    }
    public void Delete(string name, string dpi)
    {
        root = DeleteRec(root, name, dpi);
    }

    private Node DeleteRec(Node root, string name, string dpi)
    {
        // 1. Realizar eliminación normal de BST
        if (root == null) return root;

        if (string.Compare(name, root.Value.Name) < 0)
            root.Left = DeleteRec(root.Left, name, dpi);
        else if (string.Compare(name, root.Value.Name) > 0)
            root.Right = DeleteRec(root.Right, name, dpi);
        else
        {
            // Si el nodo tiene un hijo o ninguno
            if ((root.Left == null) || (root.Right == null))
            {
                Node temp = null;
                if (temp == root.Left)
                    temp = root.Right;
                else
                    temp = root.Left;

                if (temp == null)
                {
                    temp = root;
                    root = null;
                }
                else
                    root = temp;
            }
            else
            {
                // Si el nodo tiene dos hijos
                root.Value = FindMin(root.Right);
                root.Right = DeleteRec(root.Right, root.Value.Name, root.Value.Dpi);
            }
        }

        if (root == null) return root;

        // 2. Actualizar altura del nodo actual
        root.Height = 1 + Math.Max(GetHeight(root.Left), GetHeight(root.Right));

        // 3. Obtener el factor de equilibrio para ver si se desequilibró
        int balance = GetBalanceFactor(root);

        // Si el nodo se desequilibra, hay 4 casos a considerar

        // Caso LL
        if (balance > 1 && GetBalanceFactor(root.Left) >= 0)
            return RotateRight(root);

        // Caso RR
        if (balance < -1 && GetBalanceFactor(root.Right) <= 0)
            return RotateLeft(root);

        // Caso LR
        if (balance > 1 && GetBalanceFactor(root.Left) < 0)
            return RotateLeftRight(root);

        // Caso RL
        if (balance < -1 && GetBalanceFactor(root.Right) > 0)
            return RotateRightLeft(root);

        return root;
    }

    private Person FindMin(Node root)
    {
        Node current = root;
        while (current.Left != null)
            current = current.Left;

        return current.Value;
    }
    public void Update(string name, string dpi, Person updatedPerson)
    {
        // 1. Eliminar la persona con el nombre y dpi dados
        Delete(name, dpi);

        // 2. Insertar la persona actualizada
        Insert(updatedPerson);
    }
    public Person SearchByDPI(string dpi)
    {
        return SearchByDPIRec(root, dpi);
    }

    private Person SearchByDPIRec(Node node, string dpi)
    {
        if (node == null) return null;

        if (node.Value.Dpi == dpi)
            return node.Value;

        if (string.Compare(dpi, node.Value.Dpi) < 0)
            return SearchByDPIRec(node.Left, dpi);

        return SearchByDPIRec(node.Right, dpi);
    }
}
#endregion

#region Encriptar
public class MessageProcessor
{
    private readonly string _key;
    private readonly List<int> _keyOrder;

    public MessageProcessor(string key)
    {
        _key = key;
        _keyOrder = key.Select((c, i) => new { Character = c, Index = i })
                       .OrderBy(x => x.Character)
                       .Select(x => x.Index)
                       .ToList();
    }

    public string Encrypt(string input)
    {
        int numRows = (int)Math.Ceiling((double)input.Length / _key.Length);
        char[,] matrix = new char[numRows, _key.Length];

        int index = 0;

        // Llenar la matriz
        for (int row = 0; row < numRows; row++)
        {
            for (int col = 0; col < _key.Length; col++)
            {
                if (index < input.Length)
                {
                    matrix[row, col] = input[index++];
                }
                else
                {
                    matrix[row, col] = '_';  // Relleno
                }
            }
        }

        // Leer la matriz según el orden de la clave
        string encryptedText = "";
        foreach (int columnIndex in _keyOrder)
        {
            for (int row = 0; row < numRows; row++)
            {
                encryptedText += matrix[row, columnIndex];
            }
        }

        return encryptedText;
    }

    public string Decrypt(string input)
    {
        int numRows = input.Length / _key.Length;
        char[,] matrix = new char[numRows, _key.Length];
        int index = 0;

        foreach (int columnIndex in _keyOrder)
        {
            for (int row = 0; row < numRows; row++)
            {
                matrix[row, columnIndex] = input[index++];
            }
        }

        // Reconstruir el mensaje original
        string decryptedText = "";
        for (int row = 0; row < numRows; row++)
        {
            for (int col = 0; col < _key.Length; col++)
            {
                decryptedText += matrix[row, col];
            }
        }

        return decryptedText.Replace("_", "");  // Eliminar caracteres de relleno
    }
}


#endregion