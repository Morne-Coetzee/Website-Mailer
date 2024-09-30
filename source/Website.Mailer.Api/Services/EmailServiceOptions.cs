namespace Website.Mailer.Api.Services
{
    public class EmailServiceOptions
    {
        /// <summary>
        /// Use this option to redirect emails to another address, typically in a dev/testing environment.
        /// For non-production, set to a single email address, or multiple addresses separated by semicolon.
        /// Set to empty string for production environments.
        /// </summary>
        public string RedirectTo { get; set; }
    }
}
