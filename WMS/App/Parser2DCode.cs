namespace WMS.App
{
    public class Parser2DCode
    {
        public string delivery { get; set; }
        public string clientOrder { get; set; }

        public string sender { get; set; }

        public string reference { get; set; }

        public string netoWeight { get; set; }

        public string brutoWeight { get; set; }

        public string charge { get; set; }

        public string containerNo { get; set; }

        public string ident { get; set; }

        public string __helper__convertedOrder { get; set; }

        public int __helper__position { get; set; }

        public Parser2DCode(string code)
        {
            string input = code; // this is your input string
            char[] chars = input.ToCharArray();
            List<char> buffer = new List<char>();
            foreach (var c in chars)
            {
                var innerChar = c;
                if (char.IsControl(c))
                {
                    byte b = Convert.ToByte(c);
                    switch (b)
                    {
                        case 29:
                            if (buffer.Count > 0)
                            {
                                ProcessBuffer(buffer);
                                buffer.Clear();
                            }
                            break;

                        case 30:
                            break;

                        case 4:
                            if (buffer.Count > 0)
                            {
                                ProcessBuffer(buffer);
                                buffer.Clear();
                            }
                            break;
                    }
                }
                else
                {
                    buffer.Add(c);
                }
            }
        }

        private void ProcessBuffer(List<char> buffer)
        {
            Char c1 = buffer[0];
            Char c2 = buffer[1];

            if ((c2 == 'K' || c2 == 'T' || c2 == 'Q' || c2 == 'S' || c2 == 'P') && (System.Char.IsDigit(c1)))
            {
                char[] chars = { c1, c2 };
                string finalTag = new string(chars);
                switch (finalTag)
                {
                    case "2K":
                        // stevilka naklada
                        this.delivery = new string(buffer.ToArray()).Replace(finalTag, string.Empty);
                        break;

                    case "4K":
                        // pozicija
                        this.delivery = this.delivery + " / " + new string(buffer.ToArray()).Replace(finalTag, string.Empty);
                        break;

                    case "4Q":
                        this.brutoWeight = new string(buffer.ToArray()).Replace(finalTag, string.Empty);
                        break;

                    case "5Q":
                        this.netoWeight = new string(buffer.ToArray()).Replace(finalTag, string.Empty);
                        break;

                    case "3S":
                        // zaporedna stevilka palete
                        this.containerNo = string.Empty;
                        break;

                    case "1T":
                        // serijska
                        this.charge = new string(buffer.ToArray()).Replace(finalTag, string.Empty);
                        break;

                    case "1P":
                        this.ident = new string(buffer.ToArray()).Replace(finalTag, string.Empty);
                        break;
                        // 1P - ident
                }
            }
            else if ((c1 == 'K' || c1 == 'T' || c1 == 'Q' || c1 == 'S' || c1 == 'P'))
            {
                char[] chars = { c1 };
                string finalTag = new string(chars);
                switch (finalTag)
                {
                    case "K":
                        this.clientOrder = new string(buffer.ToArray()).Replace(finalTag, string.Empty);
                        break;
                }
            }
        }
    }
}