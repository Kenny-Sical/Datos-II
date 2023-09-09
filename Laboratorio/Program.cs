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
                        bTree.Insert(person);
                    }
                    if (action == "DELETE")
                    {
                    }
                    if (action == "PATCH")
                    {
                        bool updated = bTree.Update(person.Name, person.DPI, person.DateBirth, person.Address);
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
    public bool Update(string name, string dpi, string newDateBirth, string newAddress)
    {
        BTreeNode node = Search(root, dpi); // Buscamos por DPI ya que es nuestra clave principal
        if (node != null)
        {
            for (int i = 0; i < node.Count; i++)
            {
                if (node.Persons[i].DPI == dpi && node.Persons[i].Name == name)
                {
                    node.Persons[i].DateBirth = newDateBirth;
                    node.Persons[i].Address = newAddress;
                    return true; // Actualización exitosa
                }
            }
        }
        return false; // No se encontró la persona con el nombre y DPI dados
    }
    public BTreeNode Search(BTreeNode node, string dpi)
    {
        int i = 0;
        while (i < node.Count && string.Compare(dpi, node.Persons[i].DPI) > 0)
        {
            i++;
        }

        // Si encontramos el DPI en el nodo actual
        if (i < node.Count && node.Persons[i].DPI == dpi)
        {
            return node;
        }
        // Si el nodo es una hoja, significa que el DPI no está en este árbol
        else if (node.IsLeaf)
        {
            return null;
        }
        // Si el nodo no es una hoja, buscamos el DPI en el subárbol correspondiente
        else
        {
            return Search(node.Children[i], dpi);
        }
    }
    public void Delete(string dpi)
    {
        Delete(root, dpi);
    }

    private void Delete(BTreeNode node, string dpi)
    {
        int idx = 0;

        while (idx < node.Count && dpi.CompareTo(node.Persons[idx].DPI) > 0)
        {
            idx++;
        }

        // Si el valor está en este nodo y es un nodo hoja, simplemente lo eliminamos.
        if (node.IsLeaf && idx < node.Count && node.Persons[idx].DPI == dpi)
        {
            for (int i = idx + 1; i < node.Count; i++)
            {
                node.Persons[i - 1] = node.Persons[i];
            }

            node.Count--;
            node.Persons[node.Count] = null;
            return;
        }
        // Si el nodo es un nodo interno y contiene el DPI
        if (!node.IsLeaf && idx < node.Count && node.Persons[idx].DPI == dpi)
        {
            var y = node.Children[idx];   // Hijo precedente
            var z = node.Children[idx + 1]; // Hijo siguiente

            // Caso a
            if (y.Count >= t)
            {
                var pred = GetMax(y);
                node.Persons[idx] = pred;
                Delete(y, pred.DPI);
                return;
            }
            // Caso b
            if (z.Count >= t)
            {
                var succ = GetMin(z);
                node.Persons[idx] = succ;
                Delete(z, succ.DPI);
                return;
            }
            // Caso c
            Merge(node, idx);
            Delete(y, dpi);
        }
        if (!node.IsLeaf)
        {
            var child = node.Children[idx];

            // Caso 3a
            if (child.Count == t - 1 && idx > 0 && node.Children[idx - 1].Count >= t)
            {
                RotateRight(node, idx);
            }
            // Caso 3b
            else if (child.Count == t - 1 && idx < node.Count && node.Children[idx + 1].Count >= t)
            {
                RotateLeft(node, idx);
            }
            // Caso 3c
            else if (child.Count == t - 1)
            {
                // Si no es el último hijo, fusionamos con el siguiente hijo
                if (idx < node.Count)
                {
                    Merge(node, idx);
                    child = node.Children[idx];
                }
                // Si es el último hijo, fusionamos con el hijo anterior
                else
                {
                    Merge(node, idx - 1);
                    child = node.Children[idx - 1];
                }
            }

            // Llamada recursiva
            Delete(child, dpi);
        }
    }
    private Person GetMax(BTreeNode node)
    {
        while (!node.IsLeaf)
        {
            node = node.Children[node.Count];
        }
        return node.Persons[node.Count - 1];
    }

    private Person GetMin(BTreeNode node)
    {
        while (!node.IsLeaf)
        {
            node = node.Children[0];
        }
        return node.Persons[0];
    }

    private void Merge(BTreeNode node, int idx)
    {
        var child = node.Children[idx];
        var sibling = node.Children[idx + 1];

        child.Persons[t - 1] = node.Persons[idx];

        for (int i = 0; i < sibling.Count; i++)
        {
            child.Persons[i + t] = sibling.Persons[i];
        }

        if (!child.IsLeaf)
        {
            for (int i = 0; i <= sibling.Count; i++)
            {
                child.Children[i + t] = sibling.Children[i];
            }
        }

        for (int i = idx + 1; i < node.Count; i++)
        {
            node.Persons[i - 1] = node.Persons[i];
        }

        for (int i = idx + 2; i <= node.Count; i++)
        {
            node.Children[i - 1] = node.Children[i];
        }

        child.Count += sibling.Count + 1;
        node.Count--;

        // Liberar el nodo hermano
        sibling = null;
    }
    private void RotateLeft(BTreeNode node, int idx)
    {
        var child = node.Children[idx];
        var sibling = node.Children[idx + 1];

        child.Persons[child.Count] = node.Persons[idx];
        child.Count++;

        node.Persons[idx] = sibling.Persons[0];

        for (int i = 1; i < sibling.Count; i++)
        {
            sibling.Persons[i - 1] = sibling.Persons[i];
        }

        if (!sibling.IsLeaf)
        {
            child.Children[child.Count] = sibling.Children[0];
            for (int i = 1; i <= sibling.Count; i++)
            {
                sibling.Children[i - 1] = sibling.Children[i];
            }
        }

        sibling.Count--;
    }

    private void RotateRight(BTreeNode node, int idx)
    {
        var child = node.Children[idx];
        var sibling = node.Children[idx - 1];

        for (int i = child.Count; i > 0; i--)
        {
            child.Persons[i] = child.Persons[i - 1];
        }
        child.Count++;
        child.Persons[0] = node.Persons[idx - 1];

        node.Persons[idx - 1] = sibling.Persons[sibling.Count - 1];

        if (!sibling.IsLeaf)
        {
            for (int i = child.Count; i > 0; i--)
            {
                child.Children[i] = child.Children[i - 1];
            }
            child.Children[0] = sibling.Children[sibling.Count];
        }

        sibling.Count--;
    }
}
#endregion