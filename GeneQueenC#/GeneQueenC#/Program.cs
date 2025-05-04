using Spectre.Console;
Console.OutputEncoding = System.Text.Encoding.UTF8;


//int rowCount = int.Parse(Console.ReadLine());
int rowCount = 12;
List<bool[,]> states = new List<bool[,]>();
List<int[]> firstBatch = new List<int[]>();
List<int[]> secondBatch = new List<int[]>();
List<List<int[]>> nextGenBatchs = new List<List<int[]>>();
int desiredFitness = 0;

//initialize the table//
var table = new Table();
for (int i = 0; i < rowCount; i++)
{
    table.AddColumn("");
}
for (int i = 0; i < rowCount; i++)
{
    table.AddRow("");
}
table
     .Centered()
     .ShowRowSeparators()
     .HideHeaders()
     .BorderColor(Color.Aqua)
     .LeftAligned()
     .Border = TableBorder.Rounded;

//create first batch of chromosome//
Random rand = new Random();

var watch = System.Diagnostics.Stopwatch.StartNew();

for (int i = 0; i < 4; i++)
{
    int[] chromosome = new int[rowCount];
    int[] legalValues = new int[rowCount];
    for (int k = 0; k < rowCount; k++)
    {
        legalValues[k] = k;
    }

    //for (int j = 0; j < rowCount; j++)
    {

        int a = rowCount - 1;
        while (/*legalValues.All(x => x != int.MaxValue)*/ a != -1)
        {
            int index = rand.Next(0, rowCount);
            if (legalValues[index] != int.MaxValue)
            {
                chromosome[a] = legalValues[index];
                legalValues[index] = int.MaxValue;
                a--;
            }

        }
        firstBatch.Add(chromosome);
    }
    Console.WriteLine("batch: " + i);
}

//create the second batch via mitosis//
secondBatch = mitosis(firstBatch);
/*
int[] perfectChromosome = { 1, 3, 5, 7, 2, 0, 6, 4 };
Console.WriteLine(Fitness(perfectChromosome));
secondBatch[0] = perfectChromosome;
*/
firstBatch.ForEach(x => Console.WriteLine(string.Join(string.Empty, x)));
secondBatch.ForEach(x => Console.WriteLine(string.Join(string.Empty, x)));

nextGenBatchs.Add(firstBatch);
nextGenBatchs.Add(secondBatch);
//the next batches are created via crossover//
//nextGenBatchs.Add(EnterBatchCrossOver(firstBatch, secondBatch));
//nextGenBatchs.Add(EnterBatchCrossOver(firstBatch, secondBatch));

int count = 0;
int killCount = 0;
//bool[,] state = BestFitness(states);
int[] bestChromosome = BestFitness(nextGenBatchs);
bool foundPerfectChromosome = false;
while (!foundPerfectChromosome)
{
    if (Fitness(BestFitness(nextGenBatchs)) == desiredFitness)
    {
        foundPerfectChromosome = true;
        bestChromosome = BestFitness(nextGenBatchs);
        Console.WriteLine("found a perfect chromosome after: " + count);
        Console.WriteLine("there was " + killCount + " mass murders");
        break;
    }
    else
    {
        count++;
        if (count == 64)
        {
            Mutation(nextGenBatchs[rand.Next(0, nextGenBatchs.Count)][rand.Next(0, nextGenBatchs[count].Count)]);
            nextGenBatchs = KillTheBadBatches(nextGenBatchs);
            count = 0;
            killCount++;
            //Console.WriteLine("nextgen count: " + nextGenBatchs.Count);
        }
        else
        {
            nextGenBatchs.Add(EnterBatchCrossOver(nextGenBatchs[rand.Next(0, nextGenBatchs.Count)], nextGenBatchs[rand.Next(0, nextGenBatchs.Count)]));
            //nextGenBatchs.Add(mitosis(nextGenBatchs[rand.Next(0, nextGenBatchs.Count)]));
            //Console.WriteLine(count.ToString());
        }
    }
}
ShowState(CreateState(bestChromosome));


//select the best chromosome of two batches and crossover//
List<int[]> EnterBatchCrossOver(List<int[]> batch1, List<int[]> batch2)
{
    int[] fitnessScoresBatch1 = new int[batch1.Count];
    for (int i = 0; i < batch1.Count; i++)
    {
        fitnessScoresBatch1[i] = Fitness(batch1[i]);
    }

    int[] fitnessScoresBatch2 = new int[batch2.Count];
    for (int i = 0; i < batch2.Count; i++)
    {
        fitnessScoresBatch2[i] = Fitness(batch2[i]);
    }

    int alphaIndex = Array.IndexOf(fitnessScoresBatch1, fitnessScoresBatch1.Min());
    int[] alpha = batch1[alphaIndex];
    int betaIndex = Array.IndexOf(fitnessScoresBatch2, fitnessScoresBatch2.Min());
    int[] beta = batch2[betaIndex];
    //return OnePointCrossOver(batch1[alphaIndex], batch2[betaIndex]);
    //return RandomPointsCrossOver(batch1[alphaIndex], batch2[betaIndex]);
    return PositionBasedCrossover(batch1[alphaIndex], batch2[betaIndex]);
}

//select the best two chromosomes of a batch and crossover//
List<int[]> mitosis(List<int[]> batch)
{
    //List<int[]> tempBatch = batch;// remember this shit for the rest of your life you stupid nigga. this just creates a reference to the memory
    // Deep copy of batch:
    List<int[]> tempBatch = batch.Select(arr => (int[])arr.Clone()).ToList();

    List<int[]> output = new List<int[]>();
    int[] fitnessScores = new int[tempBatch.Count];
    for (int i = 0; i < tempBatch.Count; i++)
    {
        fitnessScores[i] = Fitness(tempBatch[i]);
        //Console.WriteLine("state fitness: " + fitnessScores[i]);
    }

    int[] maxValue = new int[tempBatch[0].Length];
    for (int i = 0; i < tempBatch[0].Length; i++)
    {
        maxValue[i] = int.MaxValue;
    }

    //Console.WriteLine("this batch count: " + tempBatch.Count);
    int alphaIndex = Array.IndexOf(fitnessScores, fitnessScores.Min());
    int[] alpha = tempBatch[alphaIndex];
    tempBatch[alphaIndex] = maxValue;
    fitnessScores[alphaIndex] = int.MaxValue;

    int betaIndex = Array.IndexOf(fitnessScores, fitnessScores.Min());
    int[] beta = tempBatch[betaIndex];
    tempBatch[betaIndex] = maxValue;
    fitnessScores[betaIndex] = int.MaxValue;

    int gammaIndex = Array.IndexOf(fitnessScores, fitnessScores.Min());
    int[] gamma = tempBatch[gammaIndex];
    tempBatch[gammaIndex] = maxValue;
    fitnessScores[gammaIndex] = int.MaxValue;

    int deltaIndex = Array.IndexOf(fitnessScores, fitnessScores.Min());
    int[] delta = tempBatch[deltaIndex];
    tempBatch[deltaIndex] = maxValue;

    //output.AddRange(OnePointCrossOver(alpha, beta));
    //output.AddRange(OnePointCrossOver(gamma, delta));
    output.AddRange(PositionBasedCrossover(alpha, beta));
    output.AddRange(PositionBasedCrossover(gamma, delta));

    return output;
}

//crossover two chromosomes (swap a piece a of a chromosome with another chromosom's piece from start)//
List<int[]> OnePointCrossOver(int[] father, int[] mother)
{
    int CrossOverPoint = rand.Next(father.Length);
    int[] Mchild = new int[father.Length];
    int[] Fchild = new int[mother.Length];
    List<int[]> child = new List<int[]>();
    for (int i = 0; i < CrossOverPoint; i++)
    {
        Mchild[i] = father[i];
        Fchild[i] = mother[i];
    }
    for (int i = CrossOverPoint; i < father.Length; i++)
    {
        Mchild[i] = mother[i];
        Fchild[i] = father[i];
    }
    child.Add(Mchild);
    child.Add(Fchild);
    return child;
}

//crossover two chromosomes (swap a piece a of a chromosome with another chromosom's piece from a random point)//
List<int[]> RandomPointsCrossOver(int[] father, int[] mother)
{
    int[] CrossOverPoints = new int[father.Length];
    int[] Mchild = new int[father.Length];
    int[] Fchild = new int[mother.Length];
    List<int[]> child = new List<int[]>();
    for (int i = 0; i < father.Length / 2; i++)
    {
        CrossOverPoints[i] = rand.Next(father.Length);
    }
    for (int i = 0; i < father.Length; i++)
    {
        Mchild[i] = father[i];
        Fchild[i] = mother[i];
    }
    for (int i = 0; i < CrossOverPoints.Length; i++)
    {
        Mchild[CrossOverPoints[i]] = mother[i];
        Fchild[CrossOverPoints[i]] = father[i];
    }
    //Console.WriteLine(string.Join(string.Empty, Mchild));
    //Console.WriteLine(string.Join(string.Empty, Fchild));
    child.Add(Mchild);
    child.Add(Fchild);
    return child;
}

//crossover two chromosomes (swap random positions in a chromosome with another chromosome and fill the repeated genes with the next legal gene//
List<int[]> PositionBasedCrossover(int[] father, int[] mother)
{
    int[] positions = new int[father.Length / 2];
    int[] Mchild = new int[father.Length];
    int[] Fchild = new int[mother.Length];
    List<int> validPositions = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 };

    for (int i = 0; i < positions.Length; i++)
    {
        int position = rand.Next(0, validPositions.Count);
        positions[i] = validPositions[position];
        validPositions.RemoveAt(position);
    }

    for (int i = 0; i < Mchild.Length; i++)
    {
        Mchild[i] = int.MaxValue;
        Fchild[i] = int.MaxValue;
    }

    HashSet<int> usedM = new HashSet<int>();
    HashSet<int> usedF = new HashSet<int>();

    for (int i = 0; i < positions.Length; i++)
    {
        Mchild[positions[i]] = mother[positions[i]];
        usedM.Add(mother[positions[i]]);

        Fchild[positions[i]] = father[positions[i]];
        usedF.Add(father[positions[i]]);
    }

    int mIndex = 0, fIndex = 0;

    for (int i = 0; i < Mchild.Length; i++)
    {
        if (Mchild[i] == int.MaxValue)
        {
            while (usedM.Contains(father[mIndex])) { if (mIndex < father.Length) { mIndex++; } }
            Mchild[i] = father[mIndex++];
        }

        if (Fchild[i] == int.MaxValue)
        {
            while (usedF.Contains(mother[fIndex])) { if (fIndex < mother.Length) { fIndex++; } }
            Fchild[i] = mother[fIndex++];
        }
    }
    //Console.WriteLine(string.Join("", Mchild));
    //Console.WriteLine(string.Join("", Fchild));

    return new List<int[]> { Mchild, Fchild };
}

//calculate the fitness of a state//
int Fitness(int[] chromosome)
{
    int attackingPairs = 0;
    int column = 0;
    int diagonal = 0;
    int I = 0;
    int J = 0;
    for (int i = 0; i < chromosome.Length; i++)
    {
        for (int j = i + 1; j < chromosome.Length; j++)
        {
            // Same column
            if (chromosome[i] == chromosome[j])
            {
                attackingPairs++;
                column++;
            }

            // Same diagonal
            if (Math.Abs(chromosome[i] - chromosome[j]) == Math.Abs(i - j))
            {

                attackingPairs++;
                diagonal++;
            }
            I = i;
            J = j;
        }
    }

    return attackingPairs;
}
//create a state//
bool[,] CreateState(int[] chromosome)
{
    bool[,] state = new bool[rowCount, rowCount];
    for (int i = 0; i < rowCount; i++)
    {
        state[i, chromosome[i]] = true;
    }
    return state;
}

//mutate a chromosome//
int[] Mutation(int[] chromosome)
{
    chromosome[rand.Next(0, chromosome.Length)] = rand.Next(0, chromosome.Length);
    //Console.WriteLine("mutation");
    return chromosome;
}

//when adaptability of a batch to nature is not growing after a few mating or mutation, it should be extinct//
//if the fitness number is growing then add a point to the counter. if the counter is more than 70% of the chromosome count in the batch then the batch is good to go//
bool IsGrowingAdaptability(int[] fitnesses)
{
    int growth = 0;
    for (int i = 0; i < fitnesses.Length; i++)
    {
        if (fitnesses[i] < fitnesses[i - 1]) { growth++; }
    }
    if (growth > fitnesses.Length * 0.7)
    {
        return true;
    }
    return false;
}

//find the best fitness in a list of batches//
int[] BestFitness(List<List<int[]>> batches)
{
    List<int[]> allChromosomes = new List<int[]>();
    int chromosomeCount = 0;
    for (int i = 0; i < batches.Count; i++)
    {
        for (int j = 0; j < batches[i].Count; j++)
        {
            allChromosomes.Add(batches[i][j]);
            chromosomeCount++;
        }
    }
    int[] fitnesses = new int[chromosomeCount];

    for (int i = 0; i < fitnesses.Length; i++)
    {
        fitnesses[i] = Fitness(allChromosomes[i]);
    }
    return allChromosomes[Array.IndexOf(fitnesses, fitnesses.Min())];
}

//show a state//
void ShowState(bool[,] state)
{
    for (int i = 0; i < rowCount; i++)
    {
        for (int j = 0; j < rowCount; j++)
        {
            if (state[i, j] == true)
            {
                table.UpdateCell(i, j, "♕");
            }
            else
            {
                table.UpdateCell(i, j, " ");
            }
        }
    }
    AnsiConsole.Write(table);
}

void UpdateStates(List<List<int[]>> batches)
{
    states.Clear();
    foreach (List<int[]> batch in batches)
    {
        for (int i = 0; i < batch.Count; i++)
        {
            states.Add(CreateState(batch[i]));
        }
    }

}

//kill the bad batches (fitness below 2)//
List<List<int[]>> KillTheBadBatches(List<List<int[]>> batches)
{
    List<List<int[]>> newBatches = new List<List<int[]>>();
    for (int i = 0; i < batches.Count; i++)
    {
        int[] fitnesses = new int[batches.Count];
        for (int j = 0; j < batches[i].Count; j++)
        {
            fitnesses[j] = Fitness(batches[i][j]);

        }
        if (fitnesses.Average() < desiredFitness + 2)
        {
            //Console.WriteLine(fitnesses.Average());
            newBatches.Add(batches[i]); //this should not be here//
        }
    }
    //Console.WriteLine("removed batches: " + (batches.Count - newBatches.Count));
    return newBatches;
}

watch.Stop();
var elapsedMs = watch.ElapsedMilliseconds;
Console.WriteLine("time spent: " + elapsedMs.ToString());