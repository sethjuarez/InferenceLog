
public class Prediction
{
    public string ClassName { get; set; } = "None";
    public double Score { get; set; } = 0;
    public string Machine { get; set; } = "None";
    public DateTime Timestamp { get; set; } = DateTime.Now;

    public override string ToString()
    {
        return $"{ClassName}, {Score}, {Machine}, {Timestamp}";
    }

}

public class PredictionGenerator
{
    private DiscreteDraw _classDraw;
    private DiscreteDraw _machineDraw;

    public PredictionGenerator()
    {
        _classDraw = new DiscreteDraw(
          new[] { "None", "paper", "rock", "scissors" },
          new[] { 8.0, 5, 3, 2 }
        );

        _machineDraw = new DiscreteDraw(
          new[] { "Laser Cutter", "Milling Machine", "Saw", "Plunger" },
          new[] { 8.0, 5, 3, 2 }
        );
    }

    private double Uniform(double min, double max)
    {
        return min + (max - min) * System.Random.Shared.NextDouble();
    }

    public Prediction Create()
    {
        return new Prediction()
        {
            ClassName = _classDraw.Draw(),
            Machine = _machineDraw.Draw(),
            Score = this.Uniform(.5, 1.0),
            Timestamp = DateTime.Now
        };
    }
}

public class DiscreteDraw
{
    public string[] Classes { get; set; }
    public double[] Densities { get; set; }

    public DiscreteDraw(string[] classes, double[] densities)
    {
        Classes = classes;
        Densities = densities;
        Normalize();
    }

    private void Normalize()
    {
        double sum = Densities.Sum();
        double last = 0;
        for (int i = 0; i < Densities.Length; i++)
        {
            Densities[i] /= sum;
            Densities[i] += last;
            last = Densities[i];
        }
    }

    public string Draw()
    {
        // this is random enough.... (but not the best....)
        double sample = System.Random.Shared.NextDouble();
        double last = 0;
        int index = -1;
        for (int i = 0; i < Densities.Length; i++)
        {
            if (sample > last && sample <= Densities[i])
            {
                index = i;
                break;
            }
            last = Densities[i];
        }

        if (index > -1 && index < Classes.Length)
            return Classes[index];
        else
            throw new InvalidOperationException("Bad index...");
    }
}