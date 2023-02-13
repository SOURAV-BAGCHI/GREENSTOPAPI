using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MimeKit;

namespace CommonMethodLib
{
    public class CommonMethodd
    {
         public static String ReadHtmlFile(String htmlFilePath)
        {
            StringBuilder store = new StringBuilder();

            try
            {
                using (StreamReader htmlReader = new StreamReader(htmlFilePath))
                {
                    String line;
                    while ((line = htmlReader.ReadLine()) != null)
                    {
                        store.Append(line);
                    }
                }
            }
            // catch (Exception ex)
            catch (Exception)
             { }

            return store.ToString();
        }

        public static bool SendMail(String SendereMailId,String Pwd, String Mailbody, String EmailAddresses, String Subject,String Host,Int32 Port=2525,Boolean UseSSl=false)//, String MailAttachmentFilename, String TempImageFileName)
        {
            Boolean success = false;
            if(EmailAddresses=="NONE")
            {
                return false;
            }
            try
            {
                
                MimeMessage message = new MimeMessage();

                MailboxAddress from = new MailboxAddress("GreenStop", 
                SendereMailId);
                message.From.Add(from);

                MailboxAddress to = new MailboxAddress("Admin", 
                EmailAddresses);
                message.To.Add(to);

                message.Subject = Subject;

                BodyBuilder bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = Mailbody;
                //bodyBuilder.TextBody = "Hello World!";

                message.Body = bodyBuilder.ToMessageBody();

                MailKit.Net.Smtp.SmtpClient client = new MailKit.Net.Smtp.SmtpClient();
                client.Connect(Host, Port, UseSSl);
                client.Authenticate(SendereMailId, Pwd);

                client.Send(message);
                client.Disconnect(true);
                client.Dispose();

                success = true;
            }
            // catch (Exception ex)
            catch (Exception)
            {

            }
            return success;
        }

        public static bool SendMail(String SendereMailId,String Pwd, String Mailbody, List<String> EmailAddresses, String Subject,String Host,Int32 Port=2525,Boolean UseSSl=false,List<String> Attachments=null)//, String MailAttachmentFilename, String TempImageFileName)
        {
            Boolean success = false;
            if(EmailAddresses.Count==0)
            {
                return false;
            }
            try
            {
                
                MimeMessage message = new MimeMessage();

                MailboxAddress from = new MailboxAddress("GreenStop", 
                SendereMailId);
                message.From.Add(from);

                foreach(var m in EmailAddresses)
                {
                    MailboxAddress to = new MailboxAddress("Admin", 
                    m);
                    message.To.Add(to);
                }
                

                message.Subject = Subject;

                BodyBuilder bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = Mailbody;
                //bodyBuilder.TextBody = "Hello World!";

                // if(message.Attachments!=null)
                
                if(Attachments!=null)
                {
                    foreach(var a in Attachments)
                    {
                        if(!String.IsNullOrEmpty(a))
                        {
                            bodyBuilder.Attachments.Add(a);
                        }
                        
                    }
                }
                

                message.Body = bodyBuilder.ToMessageBody();

                MailKit.Net.Smtp.SmtpClient client = new MailKit.Net.Smtp.SmtpClient();
                client.Connect(Host, Port, UseSSl);
                client.Authenticate(SendereMailId, Pwd);

                client.Send(message);
                client.Disconnect(true);
                client.Dispose();

                success = true;
            }
            // catch (Exception ex)
            catch (Exception)
            {

            }
            return success;
        }

        public static bool SendMail(String SendereMailId,String Pwd, String Mailbody, List<String> EmailAddresses, String Subject,String Host,Stream attachment,String Filename,Int32 Port=2525,Boolean UseSSl=false)//, String MailAttachmentFilename, String TempImageFileName)
        {
            Boolean success = false;
            if(EmailAddresses.Count==0)
            {
                return false;
            }
            try
            {
                
                MimeMessage message = new MimeMessage();

                MailboxAddress from = new MailboxAddress("GreenStop", 
                SendereMailId);
                message.From.Add(from);

                foreach(var m in EmailAddresses)
                {
                    MailboxAddress to = new MailboxAddress("Admin", 
                    m);
                    message.To.Add(to);
                }
                

                message.Subject = Subject;

                BodyBuilder bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = Mailbody;
                
                ContentType ct= new ContentType("application", "pdf");
                bodyBuilder.Attachments.Add(Filename,attachment,ct);
                

                message.Body = bodyBuilder.ToMessageBody();

                MailKit.Net.Smtp.SmtpClient client = new MailKit.Net.Smtp.SmtpClient();
                client.Connect(Host, Port, UseSSl);
                client.Authenticate(SendereMailId, Pwd);

                client.Send(message);
                client.Disconnect(true);
                client.Dispose();

                success = true;
            }
            // catch (Exception ex)
            catch (Exception)
            {

            }
            return success;
        }

        public static bool SendMail(String SendereMailId,String Pwd, String Mailbody, List<String> EmailAddresses, String Subject,String Host,byte [] attachment,String Filename,Int32 Port=2525,Boolean UseSSl=false)//, String MailAttachmentFilename, String TempImageFileName)
        {
            Boolean success = false;
            if(EmailAddresses.Count==0)
            {
                return false;
            }
            try
            {
                
                MimeMessage message = new MimeMessage();

                MailboxAddress from = new MailboxAddress("GreenStop", 
                SendereMailId);
                message.From.Add(from);

                foreach(var m in EmailAddresses)
                {
                    MailboxAddress to = new MailboxAddress("Admin", 
                    m);
                    message.To.Add(to);
                }
                

                message.Subject = Subject;

                BodyBuilder bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = Mailbody;

                if(attachment!=null)
                {
                    ContentType ct= new ContentType("application", "pdf");
                    bodyBuilder.Attachments.Add(Filename,attachment,ct);
                
                }
                

                message.Body = bodyBuilder.ToMessageBody();

                MailKit.Net.Smtp.SmtpClient client = new MailKit.Net.Smtp.SmtpClient();
                client.Connect(Host, Port, UseSSl);
                client.Authenticate(SendereMailId, Pwd);

                client.Send(message);
                client.Disconnect(true);
                client.Dispose();

                success = true;
            }
            // catch (Exception ex)
            catch (Exception)
            {

            }
            return success;
        }

    }
}