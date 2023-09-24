using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
class Program
{
    static void Main(string[] args)
    {
        // Crear una instancia del procesador AVL
        var processor = new AVLProcessor();

        // Procesar el archivo de bitácora
        processor.ProcessFile(@"C:\Users\sical\OneDrive\Escritorio\datos.txt");

        //Busqueda
        bool salir = true;
        while (true)
        {
            Console.WriteLine("Introduce un nombre para buscar:");
            string nameToSearch = Console.ReadLine();
            var results = processor.SearchByName(nameToSearch);
            if (results.Count == 0)
            {
                Console.WriteLine($"No se encontraron registros para el nombre: {nameToSearch}");
            }
            else
            {
                foreach (var person in results)
                {
                    Console.WriteLine($"Nombre: {person.Name}, DPI: {person.Dpi}, Fecha de Nacimiento: {person.DateBirth}, Dirección: {person.Address}");
                }
            }

            Console.WriteLine("¿Decea salir?");
            string decision = Console.ReadLine();
            if (decision == "Si")
            {
                salir = false;
            }
            else
            {
                Console.Clear();
            }
            Console.ReadKey();
        }
    }
}
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
            if (parts.Length != 2) continue;  // Invalid line format

            string operation = parts[0];
            Person person = JsonConvert.DeserializeObject<Person>(parts[1]);

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
    public List<Person> SearchByName(string name)
    {
        return avlTree.SearchByName(name);
    }
}


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
    public List<Person> SearchByName(string name)
    {
        List<Person> results = new List<Person>();
        SearchByNameRec(root, name, results);
        return results;
    }

    private void SearchByNameRec(Node node, string name, List<Person> results)
    {
        if (node == null) return;

        // Buscar en el subárbol izquierdo
        SearchByNameRec(node.Left, name, results);

        // Verificar el nodo actual
        if (node.Value.Name == name)
            results.Add(node.Value);

        // Buscar en el subárbol derecho
        SearchByNameRec(node.Right, name, results);
    }
}
#endregion