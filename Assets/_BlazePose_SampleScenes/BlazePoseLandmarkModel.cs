namespace Mediapipe.BlazePose
{
    public static class BlazePoseLandmarkModel {
        //Head
        public const int Nose = 0;
        public const int LeftEyeInner = 1;
        public const int LeftEye = 2;
        public const int LeftEyeOuter = 3;
        public const int RightEyeInner = 4;
        public const int RightEye = 5;
        public const int RightEyeOuter = 6;
        public const int LeftEar = 7;
        public const int RightEar = 8;
        public const int MouthLeft = 9;
        public const int MouthRight = 10;
        //UpperBody
        public const int LeftShoulder = 11;
        public const int RightShoulder = 12;
        public const int LeftElbow = 13;
        public const int RightElbow = 14;
        //Hands
        public const int LeftWrist = 15;
        public const int RightWrist = 16;
        public const int LeftPinky = 17;
        public const int RightPinky = 18;
        public const int LeftIndex = 19;
        public const int RightIndex = 20;
        public const int LeftThumb = 21;
        public const int RightThumb = 22;
        //LowerBody
        public const int LeftHip = 23;
        public const int RightHip = 24;
        public const int LeftKnee = 25;
        public const int RightKnee = 26;
        public const int LeftAnkle = 27;
        public const int RightAnkle = 28;
        public const int LeftHeel = 29;
        public const int RightHeel = 30;
        public const int LeftFoot = 31;
        public const int RightFoot = 32;
        //Score
        public const int Score = 33;

        // Quick handles indices
        public static readonly int[] FullBody = new int[]{ 
            Nose, 
            LeftEyeInner, LeftEye, LeftEyeOuter, RightEyeInner, RightEye, RightEyeOuter,
            LeftEar, RightEar,
            MouthLeft, MouthRight,
            LeftShoulder, RightShoulder, LeftElbow, RightElbow,
            LeftWrist, RightWrist, LeftPinky, RightPinky, LeftIndex, RightIndex, LeftThumb, RightThumb,
            LeftHip, RightHip, LeftKnee, RightKnee, LeftAnkle, RightAnkle, LeftHeel, RightHeel, LeftFoot, RightFoot
        };

        public static readonly int[] UpperBody = new int[]{
            Nose,
            LeftEyeInner, LeftEye, LeftEyeOuter, RightEyeInner, RightEye, RightEyeOuter,
            LeftEar, RightEyeOuter,
            MouthLeft, MouthRight,
            LeftShoulder, RightShoulder, LeftElbow, RightElbow,
            LeftWrist, RightWrist, LeftPinky, RightPinky, LeftIndex, RightIndex, LeftThumb, RightThumb
        };

        public static readonly int[] Hands = new int[]{
            LeftWrist, RightWrist, LeftPinky, RightPinky, LeftIndex, RightIndex, LeftThumb, RightThumb
        };
    }
}
