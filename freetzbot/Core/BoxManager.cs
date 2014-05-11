using System.Collections.Generic;
using System.Linq;
using FritzBot.Database;
using FritzBot.DataModel;

namespace FritzBot.Core
{
    public class BoxManager
    {
        private readonly User _user;
        private readonly BotContext _context;

        public BoxManager(User user, BotContext context)
        {
            _user = user;
            _context = context;
        }

        public BoxEntry AddBox(string input)
        {
            BoxEntry entry = new BoxEntry();
            entry.User = _user;
            entry.Text = input;

            Box result;
            if (BoxDatabase.GetInstance().TryFindExactBox(input, out result))
            {
                entry.Box = result;
                _context.Boxes.Attach(result);
            }

            _context.BoxEntries.Add(entry);
            _context.SaveChanges();
            return entry;
        }

        public bool RemoveBox(string input)
        {
            BoxEntry entry = _context.BoxEntries.FirstOrDefault(x => x.User.Id == _user.Id && x.Text == input);
            if (entry != null)
            {
                _context.BoxEntries.Remove(entry);
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool HasBox(string input)
        {
            Box result;
            bool found = BoxDatabase.GetInstance().TryFindExactBox(input, out result);
            if (found)
            {
                return _context.BoxEntries.Any(x => x.User.Id == _user.Id && (x.Text == input || x.Box.Id == result.Id));
            }
            return _context.BoxEntries.Any(x => x.User.Id == _user.Id && x.Text == input);
        }

        public void ReAssociateBoxes()
        {
            List<BoxEntry> userBoxEntries = _context.BoxEntries.Where(x => x.User.Id == _user.Id).ToList();
            foreach (BoxEntry entry in userBoxEntries)
            {
                Box result;
                if (BoxDatabase.GetInstance().TryFindExactBox(entry.Text, out result))
                {
                    entry.Box = result;
                }
                else
                {
                    entry.Box = null;
                }
            }
            _context.SaveChanges();
        }

        public List<string> GetRawUserBoxen()
        {
            return _context.BoxEntries.Where(x => x.User.Id == _user.Id).Select(x => x.Text).ToList();
        }

        public List<Box> GetMapAbleBoxen()
        {
            return _context.BoxEntries.Where(x => x.User.Id == _user.Id && x.Box != null).Select(x => x.Box).ToList();
        }
    }
}
