using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace HackedDesign.ClaireBot
{
    public class EnquiryDialog : ComponentDialog
    {
        // User state for enquiry dialog
        private const string NameValue = "enquiryName";
        private const string TopicValue = "enquiryTopic";

        // Prompts names
        private const string NamePrompt = "namePrompt";   
        private const string TopicPrompt = "topicPrompt";  

        private const int NameLengthMinValue = 3; 

        // Dialog IDs
        private const string DialogId = "enquiryDialog";

        public IStatePropertyAccessor<EnquiryState> UserProfileAccessor { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HackedDesign.ClaireBot.EnquiryDialog"/> class.
        /// </summary>
        /// <param name="botServices">Connected services used in processing.</param>
        /// <param name="botState">The <see cref="UserState"/> for storing properties at user-scope.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> that enables logging and tracing.</param>
        public EnquiryDialog(IStatePropertyAccessor<EnquiryState> userProfileStateAccessor, ILoggerFactory loggerFactory)
            : base(nameof(HackedDesign.ClaireBot.EnquiryDialog))
        {
            UserProfileAccessor = userProfileStateAccessor ?? throw new ArgumentNullException(nameof(userProfileStateAccessor));

            // Add control flow dialogs
            var waterfallSteps = new WaterfallStep[]
            {
                    InitializeStateStepAsync,
                    PromptForNameStepAsync,
                    SaveNameStepAsync,
                    PromptForTopicStepAsync, 
                    SaveTopicStepAsync,     
                    DisplayResultsStateStepAsync,
            };

            AddDialog(new WaterfallDialog(DialogId, waterfallSteps));
            AddDialog(new TextPrompt(NamePrompt, ValidateName));
            AddDialog(new TextPrompt(TopicPrompt, ValidateTopic));
        }

        private async Task<DialogTurnResult> InitializeStateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var enquiryState = await UserProfileAccessor.GetAsync(stepContext.Context, () => null);
            if (enquiryState == null)
            {
                var enquiryStateOpt = stepContext.Options as EnquiryState;
                if (enquiryStateOpt != null)
                {
                    await UserProfileAccessor.SetAsync(stepContext.Context, enquiryStateOpt);
                }
                else
                {
                    await UserProfileAccessor.SetAsync(stepContext.Context, new EnquiryState());
                }
            }

            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> PromptForNameStepAsync(
                                                WaterfallStepContext stepContext,
                                                CancellationToken cancellationToken)
        {
            var enquiryState = await UserProfileAccessor.GetAsync(stepContext.Context);

            // if we have everything we need, greet user and return.
            // if (enquiryState != null && !string.IsNullOrWhiteSpace(enquiryState.Name))
            // {
            //     return await GreetUser(stepContext);
            // }

            if (string.IsNullOrWhiteSpace(enquiryState.Name))
            {
                // prompt for name, if missing
                var opts = new PromptOptions
                {
                    Prompt = new Activity
                    {
                        Type = ActivityTypes.Message,
                        Text = "Firstly, can I ask who I'm talking to?",
                    },
                };
                return await stepContext.PromptAsync(NamePrompt, opts);
            }
            else
            {
                return await stepContext.NextAsync();
            }
        }

        /// <summary>
        /// Validator function to verify if the user name meets required constraints.
        /// </summary>
        /// <param name="promptContext">Context for this prompt.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        private async Task<bool> ValidateName(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            // Validate that the user entered a minimum length for their name.
            var value = promptContext.Recognized.Value?.Trim() ?? string.Empty;
            if (value.Length >= NameLengthMinValue)
            {
                promptContext.Recognized.Value = value;
                return true;
            }
            else
            {
                await promptContext.Context.SendActivityAsync($"Names needs to be at least `{NameLengthMinValue}` characters long.");
                return false;
            }
        }

        /// <summary>
        /// Validator function to verify if the user name meets required constraints.
        /// </summary>
        /// <param name="promptContext">Context for this prompt.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        private async Task<bool> ValidateTopic(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            return true;
        }
        
        private async Task<DialogTurnResult> SaveNameStepAsync(
                                                WaterfallStepContext stepContext,
                                                CancellationToken cancellationToken)
        {

            // Save name, if prompted.
            var enquiryState = await UserProfileAccessor.GetAsync(stepContext.Context);
            var updatedName = stepContext.Result as string;
            if (string.IsNullOrWhiteSpace(enquiryState.Name) && updatedName != null)
            {
                // Capitalize and set name.
                enquiryState.Name = updatedName;
                await UserProfileAccessor.SetAsync(stepContext.Context, enquiryState);
            }  

            return await stepContext.NextAsync();          
        }        

        private async Task<DialogTurnResult> PromptForTopicStepAsync(
                                                WaterfallStepContext stepContext,
                                                CancellationToken cancellationToken)
        {
            var enquiryState = await UserProfileAccessor.GetAsync(stepContext.Context);

            if (string.IsNullOrWhiteSpace(enquiryState.Topic))
            {
                // prompt for name, if missing
                var opts = new PromptOptions
                {
                    Prompt = new Activity
                    {
                        Type = ActivityTypes.Message,
                        Text = "What topic can I help you with?",
                    },
                };
                return await stepContext.PromptAsync(TopicPrompt, opts);
            }
            else
            {
                return await stepContext.NextAsync();
            }
        }

        private async Task<DialogTurnResult> SaveTopicStepAsync(
                                                WaterfallStepContext stepContext,
                                                CancellationToken cancellationToken)
        {

            // Save name, if prompted.
            var enquiryState = await UserProfileAccessor.GetAsync(stepContext.Context);
            var updatedTopic = stepContext.Result as string;
            if (string.IsNullOrWhiteSpace(enquiryState.Topic) && updatedTopic != null)
            {
                // Capitalize and set name.
                enquiryState.Topic = updatedTopic;
                await UserProfileAccessor.SetAsync(stepContext.Context, enquiryState);
            }  

            return await stepContext.NextAsync();          
        }         

        private async Task<DialogTurnResult> DisplayResultsStateStepAsync(
                                                    WaterfallStepContext stepContext,
                                                    CancellationToken cancellationToken)
        {
             return await RespondToEnquiry(stepContext);
        }

        // Helper function to greet user with information in GreetingState.
        private async Task<DialogTurnResult> RespondToEnquiry(WaterfallStepContext stepContext)
        {
            var context = stepContext.Context;
            var enquiryState = await UserProfileAccessor.GetAsync(context);

            // Display their profile information and end dialog.
            await context.SendActivityAsync($"Hi {enquiryState.Name}, give me a moment while I look for information about {enquiryState.Topic}");
            return await stepContext.EndDialogAsync();
        }  
    }    
}