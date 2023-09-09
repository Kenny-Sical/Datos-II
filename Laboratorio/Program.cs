using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laboratorio
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var path = @"C:\Users\sical\OneDrive\Escritorio\datos.txt";
            var bTree = new BTree();

            using (var reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var parts = line.Split(';');

                    var action = parts[0];
                    var jsonData = parts[1];

                    var person = JsonConvert.DeserializeObject<Person>(jsonData);

                    if (action == "INSERT")
                    {
                    }
                    if (action == "DELETE")
                    {
                    }
                    if (action == "PATCH")
                    {
                    }
                    // Para PATCH y DELETE puedes añadir lógica adicional según sea necesario
                }
            }
        }
    }
}


#region Clases Persona
public class Person
{
    public string Name { get; set; }
    public string DPI { get; set; }
    public string DateBirth { get; set; }
    public string Address { get; set; }

    public int CompareTo(Person other)
    {
        return DPI.CompareTo(other.DPI);
    }

}
public class CSVDatos
{
    public string Instruction { get; set; }
    public Person PersonData { get; set; }
}
#endregion

#region Arbol
public class BTreeNode
{
    public int Degree { get; } = 3;
    public int Count { get; set; }
    public Person[] Persons { get; set; }
    public BTreeNode[] Children { get; set; }
    public bool IsLeaf { get; set; }

    public BTreeNode()
    {
        Persons = new Person[2 * Degree - 1];
        Children = new BTreeNode[2 * Degree];
        Count = 0;
        IsLeaf = true;
    }
}
public class BTree
{
    private BTreeNode root;
    const int t = 3;
    public BTree()
    {
        root = new BTreeNode();
    }
}
#endregion