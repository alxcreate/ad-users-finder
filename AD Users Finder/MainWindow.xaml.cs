using System;
using System.Windows;
using System.DirectoryServices.AccountManagement;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace AD_Users_Finder
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void GetLogin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ButtonADGenerate_Click(sender, e);
            }
        }
        private bool ADUserNewLoginCheckFree(string login)
        {            
            using (PrincipalContext context = new PrincipalContext(ContextType.Domain))
            {
                try
                {
                    UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, login);
                    if (user == null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return true;
                }
            }
        }
        private bool ADUserNewMailCheckFree(string mail)
        {
            using (PrincipalContext context = new PrincipalContext(ContextType.Domain))
            {
                try
                {
                    UserPrincipal user = new UserPrincipal(context);

                    PrincipalSearcher searcher = new PrincipalSearcher(user);

                    foreach (UserPrincipal foundUser in searcher.FindAll())
                    {
                        if (!string.IsNullOrEmpty(foundUser.EmailAddress))
                        {
                            string[] mailPart = foundUser.EmailAddress.Split('@');
                            if (mailPart[0].ToLower() == mail)
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return true;
                }
            }
        }
        private string ADUserNewLoginGenerate(string firstName, string lastName, string delimiter, bool backwards, int letters)
        {
            string shortFirstName = string.Empty;
            if (!string.IsNullOrEmpty(firstName) && firstName.Length >= letters)
            {
                shortFirstName = firstName.Substring(0, letters);
            }

            string newLogin;
            if (backwards)
            {
                newLogin = lastName + delimiter + shortFirstName;
            }
            else
            {
                newLogin = shortFirstName + delimiter + lastName;
            }
            return newLogin;
        }
        private string ADUserNewMailGenerate(string firstName, string lastName, string delimiter, bool backwards, int postfix)
        {
            string postfixMail = postfix == 1 ? string.Empty : postfix.ToString();

            string newLogin;
            if (backwards)
            {
                newLogin = lastName + delimiter + firstName + postfixMail;
            }
            else
            {
                newLogin = firstName + delimiter + lastName + postfixMail;
            }
            return newLogin;
        }
        private string Translit(string text)
        {
            var translitMap = new Dictionary<string, string>
            {
                {"а", "a"}, {"б", "b"}, {"в", "v"}, {"г", "g"}, {"д", "d"}, {"е", "e"}, {"ё", "e"},
                {"ж", "zh"}, {"з", "z"}, {"и", "i"}, {"й", "y"}, {"к", "k"}, {"л", "l"}, {"м", "m"},
                {"н", "n"}, {"о", "o"}, {"п", "p"}, {"р", "r"}, {"с", "s"}, {"т", "t"}, {"у", "u"},
                {"ф", "f"}, {"х", "kh"}, {"ц", "ts"}, {"ч", "ch"}, {"ш", "sh"}, {"щ", "shch"}, {"ъ", ""},
                {"ы", "y"}, {"ь", ""}, {"э", "e"}, {"ю", "yu"}, {"я", "ya"}
            };

            StringBuilder output = new StringBuilder();

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (translitMap.ContainsKey(char.ToLower(c).ToString()))
                {
                    string translit = translitMap[char.ToLower(c).ToString()];

                    // If letter "e" and previous letter - "ь" or "ъ", replace to "ye"
                    if (translit == "e" && i > 0 && (text[i - 1] == 'ь' || text[i - 1] == 'ъ' || text[i - 1] == 'Ь' || text[i - 1] == 'Ъ'))
                    {
                        translit = "ye";
                    }
                    output.Append(translit);
                }
                else
                {
                    output.Append(c);
                }
            }
            return output.ToString();
        }
        private void ButtonADGenerate_Click(object sender, RoutedEventArgs e)
        {
            string firstName = Translit(TextBoxADUserFirstName.Text);
            string lastName = Translit(TextBoxADUserLastName.Text);
            string delimiter = CheckBoxADUserUsePoint.IsChecked == true ? "." : "";
            bool backwards = CheckBoxADUserBackwards.IsChecked == true;
            int letters = 1;
            int postfix = 1;

            try
            {
                while (true)
                {
                    string newLogin = ADUserNewLoginGenerate(firstName, lastName, delimiter, backwards, letters);
                    bool statusLogin = ADUserNewLoginCheckFree(newLogin);

                    if (statusLogin || letters == 10)
                    {
                        while (true)
                        {
                            string newMail = ADUserNewMailGenerate(firstName, lastName, delimiter, backwards, postfix);
                            bool statusMail = ADUserNewMailCheckFree(newMail);

                            if (statusMail || postfix == 10)
                            {
                                TextBoxADUserNewEMail.Text = newMail;
                                break;
                            }
                            postfix++;
                        }
                        TextBoxADUserNewsAMAccountName.Text = newLogin;
                        break;
                    }
                    letters++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
