using FritzBot.Database;
using FritzBot.DataModel;
using System.Collections.Generic;
using System.Linq;

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

            if (BoxDatabase.TryFindExactBox(input, out Box result))
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
            if (BoxDatabase.TryFindExactBox(input, out Box result))
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
                entry.Box = BoxDatabase.TryFindExactBox(entry.Text, out Box result) ? result : null;
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
