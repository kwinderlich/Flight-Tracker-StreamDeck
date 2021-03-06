﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SharpDeck;
using SharpDeck.Events.Received;
using SharpDeck.Manifest;
using System.Threading.Tasks;
using System.Timers;

namespace FlightStreamDeck.Logics.Actions
{
    #region Action Registration

    [StreamDeckAction("tech.flighttracker.streamdeck.master.activate")]
    public class ApMasterToggleAction : ApToggleAction
    {
        public ApMasterToggleAction(ILogger<ApMasterToggleAction> logger, IFlightConnector flightConnector, IImageLogic imageLogic)
            : base(logger, flightConnector, imageLogic) { }
    }
    [StreamDeckAction("tech.flighttracker.streamdeck.heading.activate")]
    public class ApHeadingToggleAction : ApToggleAction
    {
        public ApHeadingToggleAction(ILogger<ApHeadingToggleAction> logger, IFlightConnector flightConnector, IImageLogic imageLogic)
            : base(logger, flightConnector, imageLogic) { }
    }
    [StreamDeckAction("tech.flighttracker.streamdeck.nav.activate")]
    public class ApNavToggleAction : ApToggleAction
    {
        public ApNavToggleAction(ILogger<ApNavToggleAction> logger, IFlightConnector flightConnector, IImageLogic imageLogic)
            : base(logger, flightConnector, imageLogic) { }
    }
    [StreamDeckAction("tech.flighttracker.streamdeck.approach.activate")]
    public class ApAprToggleAction : ApToggleAction
    {
        public ApAprToggleAction(ILogger<ApAprToggleAction> logger, IFlightConnector flightConnector, IImageLogic imageLogic)
            : base(logger, flightConnector, imageLogic) { }
    }
    [StreamDeckAction("tech.flighttracker.streamdeck.altitude.activate")]
    public class ApAltToggleAction : ApToggleAction
    {
        public ApAltToggleAction(ILogger<ApAltToggleAction> logger, IFlightConnector flightConnector, IImageLogic imageLogic)
            : base(logger, flightConnector, imageLogic) { }
    }

    #endregion

    public abstract class ApToggleAction : StreamDeckAction
    {
        private readonly ILogger logger;
        private readonly IFlightConnector flightConnector;
        private readonly IImageLogic imageLogic;
        private readonly Timer timer;
        private AircraftStatus status = null;
        private string action;
        private bool timerHasTick;
        private bool legacyDisplayImage = true;

        public ApToggleAction(ILogger logger, IFlightConnector flightConnector, IImageLogic imageLogic)
        {
            this.logger = logger;
            this.flightConnector = flightConnector;
            this.imageLogic = imageLogic;
            timer = new Timer { Interval = 1000 };
            timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timerHasTick = true;
            timer.Stop();

            var currentStatus = status;
            if (currentStatus != null)
            {
                switch (action)
                {
                    case "tech.flighttracker.streamdeck.heading.activate":
                        logger.LogInformation("Toggle AP HDG. Current state: {state}.", currentStatus.IsApHdgOn);
                        flightConnector.ApHdgSet((uint)currentStatus.Heading);
                        break;
                }
            }
        }

        private async void FlightConnector_AircraftStatusUpdated(object sender, AircraftStatusUpdatedEventArgs e)
        {
            if (StreamDeck == null) return;

            var lastStatus = status;
            status = e.AircraftStatus;

            switch (action)
            {
                case "tech.flighttracker.streamdeck.master.activate":
                    if (e.AircraftStatus.IsAutopilotOn != lastStatus?.IsAutopilotOn)
                    {
                        logger.LogInformation("Received AP update: {state}", e.AircraftStatus.IsAutopilotOn);
                        await UpdateImage();
                    }
                    break;
                case "tech.flighttracker.streamdeck.heading.activate":
                    if (e.AircraftStatus.ApHeading != lastStatus?.ApHeading || e.AircraftStatus.IsApHdgOn != lastStatus?.IsApHdgOn)
                    {
                        logger.LogInformation("Received HDG update: {IsApHdgOn} {ApHeading}", e.AircraftStatus.IsApHdgOn, e.AircraftStatus.ApHeading);
                        await UpdateImage();
                    }
                    break;
                case "tech.flighttracker.streamdeck.nav.activate":
                    if (e.AircraftStatus.IsApNavOn != lastStatus?.IsApNavOn)
                    {
                        logger.LogInformation("Received NAV update: {IsApNavOn}", e.AircraftStatus.IsApNavOn);
                        await UpdateImage();
                    }
                    break;
                case "tech.flighttracker.streamdeck.approach.activate":
                    if (e.AircraftStatus.IsApAprOn != lastStatus?.IsApAprOn)
                    {
                        logger.LogInformation("Received APR update: {IsApAprOn}", e.AircraftStatus.IsApAprOn);
                        await UpdateImage();
                    }
                    break;
                case "tech.flighttracker.streamdeck.altitude.activate":
                    if (e.AircraftStatus.ApAltitude != lastStatus?.ApAltitude || e.AircraftStatus.IsApAltOn != lastStatus?.IsApAltOn)
                    {
                        logger.LogInformation("Received ALT update: {IsApHdgOn} {ApHeading}", e.AircraftStatus.IsApAltOn, e.AircraftStatus.ApAltitude);
                        await UpdateImage();
                    }
                    break;
            }
        }

        protected override async Task OnWillAppear(ActionEventArgs<AppearancePayload> args)
        {
            action = args.Action;
            status = null;
            setValues(args.Payload.Settings);
            this.flightConnector.AircraftStatusUpdated += FlightConnector_AircraftStatusUpdated;

            await UpdateImage();
        }

        private void setValues(JObject settings)
        {
            bool newLegacyDispalyImage = settings.Value<bool>("ImageDisplayTypeValue");

            legacyDisplayImage = newLegacyDispalyImage;
        }

        protected override Task OnWillDisappear(ActionEventArgs<AppearancePayload> args)
        {
            this.flightConnector.AircraftStatusUpdated -= FlightConnector_AircraftStatusUpdated;
            status = null;

            return Task.CompletedTask;
        }

        protected override Task OnKeyDown(ActionEventArgs<KeyPayload> args)
        {
            timerHasTick = false;
            string action1 = args.Action;
            action = action1;
            timer.Start();
            return Task.CompletedTask;
        }

        protected override Task OnKeyUp(ActionEventArgs<KeyPayload> args)
        {
            timer.Stop();
            if (!timerHasTick)
            {
                var currentStatus = status;
                if (currentStatus != null)
                {
                    switch (action)
                    {
                        case "tech.flighttracker.streamdeck.master.activate":
                            logger.LogInformation("Toggle AP Master. Current state: {state}.", currentStatus.IsAutopilotOn);
                            flightConnector.ApToggle();
                            break;

                        case "tech.flighttracker.streamdeck.heading.activate":
                            logger.LogInformation("Toggle AP HDG. Current state: {state}.", currentStatus.IsApHdgOn);
                            flightConnector.ApHdgToggle();
                            break;

                        case "tech.flighttracker.streamdeck.nav.activate":
                            logger.LogInformation("Toggle AP NAV. Current state: {state}.", currentStatus.IsApNavOn);
                            flightConnector.ApNavToggle();
                            break;

                        case "tech.flighttracker.streamdeck.approach.activate":
                            logger.LogInformation("Toggle AP APR. Current state: {state}.", currentStatus.IsApAprOn);
                            flightConnector.ApAprToggle();
                            break;

                        case "tech.flighttracker.streamdeck.altitude.activate":
                            logger.LogInformation("Toggle AP ALT. Current state: {state}.", currentStatus.IsApAltOn);
                            flightConnector.ApAltToggle();
                            break;

                    }
                }
            }
            timerHasTick = false;
            return Task.CompletedTask;
        }

        private async Task UpdateImage()
        {
            var currentStatus = status;
            if (currentStatus != null)
            {
                switch (action)
                {
                    case "tech.flighttracker.streamdeck.master.activate":
                        await SetImageAsync(imageLogic.GetImage("AP", currentStatus.IsAutopilotOn, legacyButtonStyle: legacyDisplayImage));
                        break;

                    case "tech.flighttracker.streamdeck.heading.activate":
                        await SetImageAsync(imageLogic.GetImage("HDG", currentStatus.IsApHdgOn, legacyButtonStyle: true, currentStatus.ApHeading.ToString()));
                        break;

                    case "tech.flighttracker.streamdeck.nav.activate":
                        await SetImageAsync(imageLogic.GetImage("NAV", currentStatus.IsApNavOn, legacyButtonStyle: legacyDisplayImage));
                        break;

                    case "tech.flighttracker.streamdeck.approach.activate":
                        await SetImageAsync(imageLogic.GetImage("APR", currentStatus.IsApAprOn, legacyButtonStyle: legacyDisplayImage));
                        break;

                    case "tech.flighttracker.streamdeck.altitude.activate":
                        await SetImageAsync(imageLogic.GetImage("ALT", currentStatus.IsApAltOn, legacyButtonStyle: true, currentStatus.ApAltitude.ToString()));
                        break;
                }
            }
        }

        protected override Task OnSendToPlugin(ActionEventArgs<JObject> args)
        {
            setValues(args.Payload);
            _ = UpdateImage();
            return Task.CompletedTask;
        }
    }
}
