using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
    public void Insert(Person person)
    {
        var r = root;
        if (r.Count == 2 * r.Degree - 1)
        {
            var s = new BTreeNode();
            root = s;
            s.IsLeaf = false;
            s.Count = 0;
            s.Children[0] = r;
            SplitChild(s, 0);
            InsertNonFull(s, person);
        }
        else
        {
            InsertNonFull(r, person);
        }
    }

    private void SplitChild(BTreeNode x, int i)
    {
        var z = new BTreeNode();
        var y = x.Children[i];
        z.IsLeaf = y.IsLeaf;
        z.Count = y.Degree - 1;

        for (int j = 0; j < y.Degree - 1; j++)
        {
            z.Persons[j] = y.Persons[j + y.Degree];
            y.Persons[j + y.Degree] = null;
        }

        if (!y.IsLeaf)
        {
            for (int j = 0; j < y.Degree; j++)
            {
                z.Children[j] = y.Children[j + y.Degree];
                y.Children[j + y.Degree] = null;
            }
        }
        y.Count = y.Degree - 1;

        for (int j = x.Count; j >= i + 1; j--)
        {
            x.Children[j + 1] = x.Children[j];
        }
        x.Children[i + 1] = z;

        for (int j = x.Count - 1; j >= i; j--)
        {
            x.Persons[j + 1] = x.Persons[j];
        }
        x.Persons[i] = y.Persons[y.Degree - 1];
        y.Persons[y.Degree - 1] = null;
        x.Count++;
    }

    private void InsertNonFull(BTreeNode x, Person person)
    {
        int i = x.Count - 1;
        if (x.IsLeaf)
        {
            while (i >= 0 && person.CompareTo(x.Persons[i]) < 0)
            {
                x.Persons[i + 1] = x.Persons[i];
                i--;
            }

            x.Persons[i + 1] = person;
            x.Count++;
        }
        else
        {
            while (i >= 0 && person.CompareTo(x.Persons[i]) < 0)
            {
                i--;
            }

            i++;

            if (x.Children[i].Count == 2 * x.Degree - 1)
            {
                SplitChild(x, i);
                if (person.CompareTo(x.Persons[i]) > 0)
                {
                    i++;
                }
            }

            InsertNonFull(x.Children[i], person);
        }
    }
}
#endregion