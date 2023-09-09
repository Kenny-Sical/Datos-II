using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laboratorio
{
    internal class Program
    {
        static void Main(string[] args)
        {
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