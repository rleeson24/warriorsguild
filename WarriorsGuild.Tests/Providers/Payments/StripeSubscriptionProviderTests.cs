using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;
using WarriorsGuild.Data.Models;
using WarriorsGuild.Providers.Payments;

namespace WarriorsGuild.Tests.Providers.Payments
{
    [TestFixture]
    public class StripeSubscriptionProviderTests
    {
        private MockRepository mockRepository;

        private Mock<UserManager<ApplicationUser>> mockApplicationUserManager;
        private Mock<Stripe.CustomerService> mockStripeCustomerService;
        private Mock<Stripe.SubscriptionService> mockStripeSubscriptionService;
        private Mock<Stripe.InvoiceItemService> mockStripeInvoiceItemService;
        private Mock<Stripe.CardService> mockStripeCardService;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository( MockBehavior.Strict );

            this.mockApplicationUserManager = this.mockRepository.Create<UserManager<ApplicationUser>>();
            this.mockStripeCustomerService = this.mockRepository.Create<Stripe.CustomerService>();
            this.mockStripeSubscriptionService = this.mockRepository.Create<Stripe.SubscriptionService>();
            this.mockStripeInvoiceItemService = this.mockRepository.Create<Stripe.InvoiceItemService>();
            this.mockStripeCardService = this.mockRepository.Create<Stripe.CardService>();
        }

        [TearDown]
        public void TearDown()
        {
            this.mockRepository.VerifyAll();
        }

        private StripeSubscriptionProvider CreateProvider()
        {
            return new StripeSubscriptionProvider(
                this.mockApplicationUserManager.Object,
                this.mockStripeCustomerService.Object,
                this.mockStripeSubscriptionService.Object,
                this.mockStripeInvoiceItemService.Object,
                this.mockStripeCardService.Object );
        }
    }
}
