using FritzBot.Core;
using System.Collections.Generic;
using System.Linq;

namespace FritzBot.DataModel.LegacyModels
{
    public class BoxEntry : LinkedData<User>
    {
        public Dictionary<string, Box> Entrys { get; set; }
        public int Count { get { return Entrys.Count; } }

        public BoxEntry()
        {
            Entrys = new Dictionary<string, Box>();
        }

        //public void AddBox(string input)
        //{
        //    Box result;
        //    if (BoxDatabase.GetInstance().TryFindExactBox(input, out result))
        //    {
        //        Entrys.Add(input, result);
        //    }
        //    else
        //    {
        //        Entrys.Add(input, null);
        //    }
        //}

        //public bool RemoveBox(string input)
        //{
        //    KeyValuePair<string, Box> found = Entrys.FirstOrDefault(x => x.Key == input);

        //    if (String.IsNullOrEmpty(found.Key))
        //    {
        //        Box result;
        //        if (BoxDatabase.GetInstance().TryFindExactBox(input, out result))
        //        {
        //            found = Entrys.FirstOrDefault(x => x.Value == result);
        //        }
        //    }

        //    if (!String.IsNullOrEmpty(found.Key) && Entrys.Contains(found))
        //    {
        //        Entrys.Remove(found.Key);
        //        return true;
        //    }
        //    return false;
        //}

        //public bool HasBox(string input)
        //{
        //    if (GetRawUserBoxen().Contains(input))
        //    {
        //        return true;
        //    }
        //    Box result;
        //    return BoxDatabase.GetInstance().TryFindExactBox(input, out result) && GetMapAbleBoxen().Contains(result);
        //}

        //public void ReAssociateBoxes()
        //{
        //    Dictionary<string, Box> tmp = new Dictionary<string, Box>();
        //    foreach (KeyValuePair<string, Box> item in Entrys.Distinct(x => x.Key).OrderBy(x => x.Key))
        //    {
        //        Box result;
        //        if (BoxDatabase.GetInstance().TryFindExactBox(item.Key, out result))
        //        {
        //            tmp.Add(item.Key, result);
        //        }
        //        else
        //        {
        //            tmp.Add(item.Key, null);
        //        }
        //    }
        //    Entrys = tmp;
        //}

        public IEnumerable<string> GetRawUserBoxen()
        {
            return Entrys.Select(x => x.Key);
        }

        public IEnumerable<Box> GetMapAbleBoxen()
        {
            return Entrys.Select(x => x.Value).NotNull();
        }
    }
}