﻿using System;
using System.Threading;

namespace Kingsland.PiFaceSharp.PinControllers
{

    /// <summary>
    /// Implements a pin controller that automatically switches between on and off
    /// at a regular interval. This can be used to control a blinking LED attached
    /// to a PiFace Digital output pin.
    /// </summary>
    public sealed class BlinkingPinController : BackgroundPinController
    {

        #region Fields

        private byte _outputPin;
        private int _interval;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialises a BlinkController bound to the specified pin on a PiFace Digital
        /// device.
        /// </summary>
        /// <param name="piface"></param>
        /// <param name="outputPin"></param>
        /// <param name="interval">
        /// The number of milliseconds between turning the output pin on and off.
        /// </param>
        public BlinkingPinController(IPiFaceDevice piface, byte outputPin, int interval)
            : base(piface)
        {
            this.OutputPin = outputPin;
            this.Interval = interval;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the index of the output pin this controller is bound to.
        /// </summary>
        public byte OutputPin
        {
            get
            {
                return _outputPin;
            }
            private set
            {
                if (_outputPin > 7)
                {
                    throw new ArgumentOutOfRangeException("value", "Value must be 7 or less.");
                }
                _outputPin = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Interval
        {
            get
            {
                return _interval;
            }
            private set
            {
                if (value < 10)
                {
                    // there's a practical limit to how fast the output pins can be updated
                    throw new ArgumentOutOfRangeException("value", "Value must be 10 or less.");
                }
                _interval = value;
            }
        }
        
        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        protected override void Execute()
        {
            var enabled = false;
            // run the main loop
            while (this.Status == BackgroundPinControllerStatus.Running)
            {
                this.PiFace.SetOutputPinState(this.OutputPin, enabled);
                Thread.Sleep(Interval);
                enabled = !enabled;
            }
        }

        #endregion

    }

}
