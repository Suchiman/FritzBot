using FritzBot.Core;
using FritzBot.DataModel;
using System;
using System.Linq;

namespace FritzBot.Plugins
{
    [Module.Name("user")]
    [Module.Help("Führt Operationen an meiner Benutzerdatenbank aus, Operator Befehl: !user remove, reload, flush, add <name>, box <name> <box>")]
    [Module.ParameterRequired]
    [Module.Authorize]
    class user : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            try
            {
                if (theMessage.CommandArgs[0] == "add")
                {
                    User u = new User();
                    u.LastUsedName = theMessage.CommandArgs[1];
                    u.Names.Add(theMessage.CommandArgs[1]);
                    using (DBProvider db = new DBProvider())
                    {
                        db.SaveOrUpdate(u);
                    }
                    theMessage.Answer("User hinzugefügt");
                }
                if (theMessage.CommandArgs[0] == "box")
                {
                    using (DBProvider db = new DBProvider())
                    {
                        User u = db.GetUser(theMessage.CommandArgs[1]);
                        if (u != null)
                        {
                            BoxEntry entry = db.QueryLinkedData<BoxEntry, User>(u).FirstOrDefault();
                            if (entry == null)
                            {
                                entry = new BoxEntry();
                                entry.Reference = u;
                            }
                            entry.AddBox(String.Join(" ", theMessage.CommandArgs.Skip(2)));
                            db.SaveOrUpdate(entry);
                            theMessage.Answer("Hinzugefügt");
                        }
                        theMessage.Answer("User gibbet nicht");
                    }
                }
                if (theMessage.CommandArgs[0] == "remove")
                {
                    using (DBProvider db = new DBProvider())
                    {
                        User u = db.GetUser(theMessage.CommandArgs[1]);
                        if (u != null)
                        {
                            db.Remove(u);
                            theMessage.Answer("Entfernt!");
                        }
                        else
                        {
                            theMessage.Answer("Nicht gefunden");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                toolbox.Logging("Bei einer Datenbank Operation ist eine Exception aufgetreten: " + ex.Message);
                theMessage.Answer("Wups, das hat eine Exception verursacht");
            }
        }
    }
}