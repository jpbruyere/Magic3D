using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using OpenTK;

namespace Magic3D
{
	public delegate void AnimationEventHandler(Animation a);

    public delegate float GetterDelegate();
    public delegate void SetterDelegate(float value);

	public class AnimationList : List<AnimationList> ,IAnimatable
	{
		#region IAnimatable implementation

		public event EventHandler<EventArgs> AnimationFinished;

		public void Animate (float ellapseTime = 0f)
		{
			throw new NotImplementedException ();
		}

		#endregion


	}

    public class Animation
    {

		public event AnimationEventHandler AnimationFinished;

		public static int DelayMs = 0;

        protected GetterDelegate getValue;
        protected SetterDelegate setValue;

        public string propertyName;

        protected Stopwatch timer = new Stopwatch();
        protected int delayMs = 0;
        protected static List<Animation> AnimationList = new List<Animation>();

        //public FieldInfo member;
        public Object AnimatedInstance;

		public static void StartAnimation(Animation a, int delayMs = 0, AnimationEventHandler OnEnd = null)
        {

			Animation aa = null;
			if (Animation.GetAnimation (a.AnimatedInstance, a.propertyName, ref aa)) {
				aa.CancelAnimation ();
			}

			//a.AnimationFinished += onAnimationFinished;

			a.AnimationFinished += OnEnd;
			a.delayMs = delayMs + DelayMs;


            if (a.delayMs > 0)
                a.timer.Start();
            
			AnimationList.Add (a);

        }

        static Stack<Animation> anims = new Stack<Animation>();

        public static void ProcessAnimations()
        {
            //Stopwatch animationTime = new Stopwatch();
            //animationTime.Start();
			 
			const int maxAnim = 200000;
			int count = 0;


			lock (AnimationList) {
				if (anims.Count == 0)
					anims = new Stack<Animation> (AnimationList);
			}
        
			while (anims.Count > 0 && count < maxAnim) {
				Animation a = anims.Pop ();	
				if (a.timer.IsRunning) {
					if (a.timer.ElapsedMilliseconds > a.delayMs)
						a.timer.Stop ();
					else
						continue;
				}

				a.Process ();
				count++;
			}
            //animationTime.Stop();
            //Debug.WriteLine("animation: {0} ticks \t {1} ms ", animationTime.ElapsedTicks,animationTime.ElapsedMilliseconds);
        }
        public static bool GetAnimation(object instance, string PropertyName, ref Animation a)
        {
			for (int i = 0; i < AnimationList.Count; i++) {
				Animation anim = AnimationList [i];
				if (anim.AnimatedInstance == instance && anim.propertyName == PropertyName) {
					a = anim;
					return true;
				}
			}

            return false;
        }
        public virtual void Process(){}
        public void CancelAnimation()
        {
			//Debug.WriteLine("Cancel anim: " + this.ToString()); 
            AnimationList.Remove(this);
        }
		public void RaiseAnimationFinishedEvent()
		{
			if (AnimationFinished != null)
				AnimationFinished (this);
		}

		public static void onAnimationFinished(Animation a)
		{
			Debug.WriteLine ("\t\tAnimation finished: " + a.ToString ());
		}

		public override string ToString ()
		{
			return string.Format ("{0},{1}",(this.AnimatedInstance as CardInstance).Model.Name,
				this.propertyName);
		}
    }
    public class FloatAnimation : Animation
    {

        public float TargetValue;
        public float Step;


        public FloatAnimation(Object instance, string _propertyName, float Target, float step = 0.2f)
        {
            propertyName = _propertyName;
            AnimatedInstance = instance;
            PropertyInfo pi = instance.GetType().GetProperty(propertyName);
            TargetValue = Target;

            getValue = (GetterDelegate)Delegate.CreateDelegate(typeof(GetterDelegate), instance, pi.GetGetMethod());
            setValue = (SetterDelegate)Delegate.CreateDelegate(typeof(SetterDelegate), instance, pi.GetSetMethod());

            float value = getValue();

            Step = step;

            if (value < TargetValue)
            {
                if (Step < 0)
                    Step = -Step;
            }
            else if (Step > 0)
                Step = -Step;            
        }

        /// <summary>
        /// process one frame
        /// </summary>
        public override void Process()
        {
            float value = getValue();

			//Debug.WriteLine ("Anim: {0} <= {1}", value, this.ToString ());

            if (Step > 0f)
            {
                value += Step;
                setValue(value);
                //Debug.WriteLine(value);
                if (TargetValue > value)
                    return;
            }
            else
            {
                value += Step;
                setValue(value);

                if (TargetValue < value)
                    return;
            }

            setValue(TargetValue);
            AnimationList.Remove(this);

			RaiseAnimationFinishedEvent ();
        }

		public override string ToString ()
		{
			return string.Format ("{0}:->{1}:{2}",base.ToString(),TargetValue,Step);
		}
    }

    public class AngleAnimation : FloatAnimation
    {
        public AngleAnimation(Object instance, string PropertyName, float Target, float step = 0.1f) : 
            base(instance,PropertyName,Target,step)
        {
        }
        public override void Process()
        {
            base.Process();

            float value = getValue();
            if (value < -MathHelper.TwoPi)
                setValue(value + MathHelper.TwoPi);
            else if (value >= MathHelper.TwoPi)
                setValue(value - MathHelper.TwoPi);
        }
    }
}
