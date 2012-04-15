using System;
using System.Collections.Generic;
using System.Text;

namespace FritzBot.commands
{
    class calc : ICommand
    {
        public String[] Name { get { return new String[] { "calc" }; } }
        public String HelpText { get { return "Ich kann sogar Rechnen :-) !calc 42*13+1 !calc 42*(42-(24+24)+1*3)/2"; } }
        public Boolean OpNeeded { get { return false; } }
        public Boolean ParameterNeeded { get { return true; } }
        public Boolean AcceptEveryParam { get { return false; } }

        public void Destruct()
        {

        }

        public void Run(Irc connection, String sender, String receiver, String message)
        {
            try
            {
                message = message.Replace(" ", ""); //10+5-8*3/2
                if (message.Contains("("))// 100*(100+(50-25)*-3)/2      100*(50-25)+(50-25)
                {
                    while (message.Contains("("))
                    {
                        int start = message.LastIndexOf('(');
                        int end = message.Remove(0, start).IndexOf(')') + 1 + start;
                        String first = message.Remove(start);
                        String second = message.Remove(0, start + 1).Remove(end - start - 2);
                        second = CalcPartial(second);
                        String last = message.Remove(0, end);
                        message = first + second + last;
                    }
                }
                String result = CalcPartial(message);
                connection.Sendmsg("Ergebnis: " + result, receiver);
            }
            catch
            {
                connection.Sendmsg("Schade, das hat leider eine Exception bei der Verarbeitung ausgelöst...", receiver);
            }
        }

        private static String CalcPartial(String to_calc)
        {
            Char[] messageArray = to_calc.ToCharArray();
            List<String> numbers = new List<String>();
            List<Char> opers = new List<Char>();

            //Operatoren ermitteln und zur Liste hinzufügen
            int i = 0; //Ein Zeichen direkt hinter einem Operator ignorieren z.b. 10*-1
            foreach (Char onechar in messageArray)
            {
                if ((onechar == '+' || onechar == '-' || onechar == '*' || onechar == '/' || onechar == '%') && i == 0)
                {
                    opers.Add(onechar);
                    i = 2;
                }
                if (i > 0)
                {
                    i--;
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

        private static String CalcOperators(List<Char> opers, List<String> numbers)
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

        private static String CalcString(String number1, String number2, Char op)
        {
            Double num1;
            Double num2;
            String result = "";
            if (!Double.TryParse(number1, out num1))
            {
                num1 = 0;
            }
            if (!Double.TryParse(number2, out num2))
            {
                num2 = 0;
            }
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
                    throw new ArgumentException("Unknown Operator");
            }
            return result;
        }
    }
}