using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleNonNumeric
{
    public class TextInput
    {
        public string str;
        public string value;


        public virtual void Add(char c)
        {
            string str = c.ToString();
        }

        public string GetValue()
        {
            //this.Add();
            var returnstring1= value;
            return returnstring1;
        }
    }

    public class NumericInput : TextInput
    {
        public override void Add(char c)
        {

            var nonNumeric = Char.IsNumber(c);
            if (nonNumeric)
            {
                 value = value +""+ c;
            }
        }

    }
    class Program
    {
        static void Main(string[] args)
        {

            TextInput input = new NumericInput();
            input.Add('1');
            input.Add('a');
            input.Add('0');
            Console.WriteLine(input.GetValue());
            Console.ReadLine();
        }
    }
}
