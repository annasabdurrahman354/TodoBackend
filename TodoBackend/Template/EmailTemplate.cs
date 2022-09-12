using System;

namespace TodoBackend.Template
{
    public class EmailTemplate
    {
        public String EmailVerification(String confirmationLink)
        {
            String template = "<html>" +
                            "<body> <h3>Verify your email by clicking this link: </h3> <p>" + confirmationLink + "</p> </body>" +
                            "</html>";
            return template;
        }
        public String ForgetPassword(String confirmationLink)
        {
            String template = "<html>" +
                    "<body> <h3>Change your account password by clicking this link: </h3> <p>" + confirmationLink + "</p> </body>" +
                    "</html>";
            return template;
        }
    }
    
}
