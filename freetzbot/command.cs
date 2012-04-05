using System;

namespace freetzbot
{
    public interface command
    {
        String[] get_name();
        String get_helptext();
        Boolean get_op_needed();
        Boolean get_parameter_needed();
        Boolean get_accept_every_param();
        void run(irc connection, String sender, String receiver, String message);
        void destruct();
    }
}