namespace iTeffa.Infodata
{
    public class ParentData
    {
        public int Father;
        public int Mother;
        public float Similarity;
        public float SkinSimilarity;

        public ParentData(int father, int mother, float similarity, float skinsimilarity)
        {
            Father = father;
            Mother = mother;
            Similarity = similarity;
            SkinSimilarity = skinsimilarity;
        }
    }
}
