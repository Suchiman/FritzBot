using System;
using System.Collections.Generic;
using System.Text;

namespace freetzbot.commands
{
    class calc : command
    {
        private String[] name = { "calc" };
        private String helptext = "Ich kann sogar Rechnen :-) !calc 42*13+1 !calc 42*(42-(24+24)+1*3)/2";
        private Boolean op_needed = false;
        private Boolean parameter_needed = true;
        private Boolean accept_every_param = false;

        public String[] get_name()
        {
            return name;
        }

        public String get_helptext()
        {
            return helptext;
        }

        public Boolean get_op_needed()
        {
            return op_needed;
        }

        public Boolean get_parameter_needed()
        {
            return parameter_needed;
        }

        public Boolean get_accept_every_param()
        {
            return accept_every_param;
        }

        public void destruct()
        {

        }

        public void run(irc connection, String sender, String receiver, String message)
        {
            message = message.Replace(" ", ""); //10+5-8*3/2
            if (message.Contains("("))// 100*(100+(50-25)*3)/2      100*(50-25)+(50-25)
            {
                while (message.Contains("("))
                {
                    int start = message.LastIndexOf('(');
                    int end = message.IndexOf(')') + 1;
                    String first = message.Remove(start);
                    String second = message.Remove(0, start + 1).Remove(end - start - 2);
                    second = CalcPartial(second);
                    String last = message.Remove(0, end);
                    message = first + second + last;
                }
            }
            String result = CalcPartial(message);
            connection.sendmsg("Ergebnis: " + result, receiver);
        }

        private String CalcPartial(String to_calc)
        {
            Char[] messageArray = to_calc.ToCharArray();
            List<String> numbers = new List<String>();
            List<Char> opers = new List<Char>();

            //Operatoren ermitteln und zur Liste hinzufügen
            foreach (Char onechar in messageArray)
            {
                if (onechar == '+' || onechar == '-' || onechar == '*' || onechar == '/' || onechar == '%')
                {
                    opers.Add(onechar);
                }
            }

            //Zahlen finden und zur Liste hinzufügen
            foreach (Char onechar in opers)
            {
                numbers.Add(to_calc.Remove(to_calc.IndexOf(onechar)));
                to_calc = to_calc.Remove(0, to_calc.IndexOf(onechar) + 1);
            }
            numbers.Add(to_calc);

            return CalcOperators(opers, numbers);
        }

        private String CalcOperators(List<Char> opers, List<String> numbers)
        {
            //Priorisierte operatoren ( * / ) zuerst berechnen
            for (int i = 0; i < opers.Count; i++)
            {
                if (opers[i] == '*' || opers[i] == '/')
                {
                    numbers[i + 1] = CalcString(numbers[i], numbers[i + 1], opers[i]);
                    opers.RemoveAt(i);
                    numbers.RemoveAt(i);
                    i--;
                }
            }

            //Alles andere Berechnen
            foreach (Char onechar in opers)
            {
                numbers[1] = CalcString(numbers[0], numbers[1], onechar);
                numbers.RemoveAt(0);
            }
            return numbers[0];
        }

        private String CalcString(String number1, String number2, Char op)
        {
            Int64 num1 = 0;
            Int64 num2 = 0;
            String result = "";
            Int64.TryParse(number1, out num1);
            Int64.TryParse(number2, out num2);
            switch (op)
            {
                case '+':
                    result = (num1 + num2).ToString();
                    break;
                case '-':
                    result = (num1 - num2).ToString();
                    break;
                case '*':
                    result = (num1 * num2).ToString();
                    break;
                case '/':
                    result = (num1 / num2).ToString();
                    break;
                case '%':
                    result = (num1 * num2 / 100).ToString();
                    break;
                default:
                    throw new Exception("Unknown Operator");
            }
            return result;
        }
    }
}