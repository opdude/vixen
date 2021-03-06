﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms.VisualStyles;
using Vixen.Data.Value;
using Vixen.Intent;
using Vixen.Sys;
using Vixen.Sys.Dispatch;

// By no means does this need to be implemented this way.
// By all means, go ahead and do casting.
// They've so drilled into our heads at my job a fear of conditional constructs
//  that I've actually grown fearful of using them, as if I will break a constant
//  in the universe if I go too far with them.

namespace VixenModules.Editor.TimedSequenceEditor
{
	internal class IntentRasterizer : IntentDispatch, IDisposable
	{
		private RectangleF _rect;
		private Graphics _graphics;
		private TimeSpan _startOffset;
		private TimeSpan _endTime;
		private readonly TimeSpan _oneTick = TimeSpan.FromTicks(1);

		public void Rasterize(IIntent intent, RectangleF rect, Graphics g, TimeSpan startOffset, TimeSpan endTime)
		{
			// As recommended by R#
			if (Math.Abs(rect.Width - 0) < float.Epsilon || Math.Abs(rect.Height - 0) < float.Epsilon) return;

			_rect = rect;
			_graphics = g;
			_startOffset = startOffset;
			_endTime = endTime;

			intent.Dispatch(this);
		}

		private void DrawGradient(Color startColor, Color endColor, RectangleF rectangle)
		{
			// Why we have to do this? I have no idea, but without it, the gradient rendering gives strange artefacts.
			// (If you want to see what I mean, make a long spin (minutes) across a bunch of elements in a group with
			// a simple pulse down (or up). The ends/starts of the effect flip to the color of the other end briefly,
			// for a single pixel width. I'm guessing it's an issue in the gradient rendering for large shapes where
			// the gradient rectangle is within the same integer range as the rendering rectangle.
			float offset = rectangle.X*0.004F;
			RectangleF gradientRectangle = new RectangleF(
				(rectangle.X) - offset,
				rectangle.Y,
				(rectangle.Width) + (2*offset),
				rectangle.Height
				);

			if (startColor == endColor) {
				using (SolidBrush brush = new SolidBrush(startColor)) {
					_graphics.FillRectangle(brush, rectangle);
				}
			} else {
				using (LinearGradientBrush brush = new LinearGradientBrush(
					gradientRectangle, startColor, endColor, LinearGradientMode.Horizontal))
				{
					brush.GammaCorrection = true;
					_graphics.FillRectangle(brush, rectangle);
				}
			}
		}


		public void DrawStaticArrayIntent(TimeSpan timespan, RectangleF rectangle, Func<TimeSpan, Color> startColorGetter, Func<TimeSpan, Color> endColorGetter)
		{
			// StaticArrayIntents are fundementally sampled values (not gradients)
			// so we try to improve their appearance by creating more samples to rasterize.
			// The question is how many... each takes precious UI time to render.
			// For now try almost easiest, fixed num per sec
			// TODO: can we figure out what dimensions we're rendering for, and then rasterize based on that?  That's the ideal solution: one chunk per 2 pixels or so.
			int nChunks = 1 + (int)(timespan -_startOffset).TotalMilliseconds/100;
			TimeSpan tsStart = _startOffset;//TimeSpan.Zero;
			RectangleF drawRectangle = new RectangleF(rectangle.Location, rectangle.Size);
			float rectWidth = rectangle.Width/nChunks;
			for (int i = 1; i <= nChunks; i++) {
				var tsEnd = TimeSpan.FromMilliseconds((timespan-_startOffset).TotalMilliseconds / nChunks * i) + _startOffset;

				Color startColor = startColorGetter(tsStart);
				Color endColor = startColorGetter(tsEnd);

				float rectX = rectangle.X + (i - 1) * rectWidth;
				drawRectangle.X = rectX;
				drawRectangle.Width = rectWidth;

				DrawGradient(startColor, endColor, drawRectangle);

				tsStart = tsEnd;
			}
		}

		public override void Handle(IIntent<LightingValue> obj)
		{
			if (obj is StaticArrayIntent<LightingValue>) {
				Func<TimeSpan, Color> scg = x => obj.GetStateAt(x).TrueFullColorWithAlpha;
				Func<TimeSpan, Color> ecg = x => obj.GetStateAt(x - _oneTick).TrueFullColorWithAlpha;
				DrawStaticArrayIntent(_endTime, _rect, scg, ecg);
			} else {
				Color startColor = obj.GetStateAt(_startOffset).TrueFullColorWithAlpha;
				Color endColor = obj.GetStateAt(_endTime - (_endTime<obj.TimeSpan?TimeSpan.Zero:_oneTick)).TrueFullColorWithAlpha;
				DrawGradient(startColor, endColor, _rect);
			}
		}

		public override void Handle(IIntent<RGBValue> obj)
		{
			if (obj is StaticArrayIntent<RGBValue>) {
				Func<TimeSpan, Color> scg = x => obj.GetStateAt(x).ColorWithAplha;
				Func<TimeSpan, Color> ecg = x => obj.GetStateAt(x - _oneTick).ColorWithAplha;
				DrawStaticArrayIntent(_endTime, _rect, scg, ecg);
			} else {
				Color startColor = obj.GetStateAt(_startOffset).ColorWithAplha;
				Color endColor = obj.GetStateAt(_endTime - (_endTime < obj.TimeSpan ? TimeSpan.Zero : _oneTick)).ColorWithAplha;
				DrawGradient(startColor, endColor, _rect);
			}
		}

		~IntentRasterizer()
		{
			Dispose(false);
		}
		protected void Dispose(bool disposing) {
			if (disposing) { }
			if (_graphics != null) 
				_graphics.Dispose();
		}
		public void Dispose() {
			Dispose(true);
		}
	}
}