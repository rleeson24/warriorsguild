declare var isGuardian;
declare var isAdmin;
namespace WarriorsGuild {
	export class WgStripe {
		constructor() {
			this.config = new WgStripeConfig();
		}

		prepareStripeControls = function (publishableKey: string) {
			if ((typeof isGuardian === 'undefined' || !isGuardian) && (typeof isAdmin === 'undefined' || !isAdmin)) return;
			var self = this;

			this.stripe = Stripe(publishableKey);
			var stripe = this.stripe;

			var style = {
				base: {
					// Add your base input styles here. For example:
					fontSize: '16px',
					color: "#32325d",
				}
			};

			var elementStyles = {
				base: {
					color: "#32325D",
					fontWeight: 500,
					fontFamily: "Inter UI, Open Sans, Segoe UI, sans-serif",
					fontSize: "16px",
					fontSmoothing: "antialiased",

					"::placeholder": {
						color: "#CFD7DF"
					}
				},
				invalid: {
					color: "#E25950"
				}
			};

			var elementClasses = {
			};

			var elements = stripe.elements({
				fonts: [
					{
						cssSrc: 'https://fonts.googleapis.com/css?family=Quicksand',
					},
				],
				// Stripe's examples are localized to specific languages, but if
				// you wish to have Elements automatically detect your user's locale,
				// use `locale: 'auto'` instead.
				locale: 'auto'//window.__exampleLocale,
			});


			var card = elements.create('card', {
				style: elementStyles,
				classes: elementClasses,
			});
			card.mount('#card-element');

			// Handle real-time validation errors from the card Element.
			card.addEventListener('change', function (event) {
				var displayError = document.getElementById('card-errors');
				if (event.error) {
					displayError.textContent = event.error.message;
				} else {
					displayError.textContent = '';
				}
			});

			// Handle form submission.
			var errorElement = document.getElementById('card-errors');
			var paymentForm = document.querySelector(self.config.formId + ' .example');
			var form = document.querySelector(self.config.formId + ' form');
			form.addEventListener('submit', function (event) {
				event.preventDefault();

				// Show a loading screen...
				paymentForm.classList.add('submitting');

				// Disable all inputs.
				disableInputs();

				stripe.createToken(card).then(function (result) {
					if (result.error) {
						// Inform the user if there was an error.
						errorElement.textContent = result.error.message;
						paymentForm.classList.remove('submitting');
					} else {
						// Send the token to your server.
						self.config.tokenHandler(result.token.id);
					}
				});
			});

			function disableInputs() {
				Array.prototype.forEach.call(
					form.querySelectorAll(
						"input[type='text'], input[type='email'], input[type='tel']"
					),
					function (input) {
						input.setAttribute('disabled', 'true');
					}
				);
			}
		}

		beginSubmit = (): void => {

		}

		endSubmitSuccess = (): void => {
			var self = this;
			var paymentForm = document.querySelector(self.config.formId + ' .example');
			paymentForm.classList.remove('submitting');
			paymentForm.classList.add('submitted');
			self.enableInputs();
		}

		endSubmitFailure = (errorMsg: string): void => {
			var self = this;
			var paymentForm = document.querySelector(self.config.formId + ' .example');
			paymentForm.classList.remove('submitting');
			// Inform the user if there was an error.
			var errorElement = document.getElementById('card-errors');
			errorElement.textContent = errorMsg;
			self.enableInputs();
		}

		private enableInputs = (): void => {
			var self = this;
			var form = document.querySelector(self.config.formId + ' form');
			Array.prototype.forEach.call(
				form.querySelectorAll(
					"input[type='text'], input[type='email'], input[type='tel']"
				),
				function (input) {
					input.removeAttribute('disabled');
				}
			);
		}

		public config: WgStripeConfig;
	}
	export class WgStripeStatuses {
		public static readonly STATUS_OPTION_INCOMPLETE = 'Incomplete';
		public static readonly STATUS_OPTION_CREATED = 'Created';
		public static readonly STATUS_OPTION_ACTIVE = 'Active';
		public static readonly STATUS_OPTION_INACTIVE = 'Inactive';
		public static readonly STATUS_OPTION_DELETED = 'Deleted';
	}

	export class WgStripeConfig {
		formId: string = '#payment-form';
		tokenHandler: Function;
	}
}