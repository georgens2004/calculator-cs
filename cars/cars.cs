using System;

namespace CarsApp
{
    enum CarType
    {
        Tesla,
        BMW,
        Toyota,
        Audi
    }

    interface ICar
    {
        string Brand { get; }
        int Seats { get; }
        string Multimedia { get; }
        string GetDescription();
    }

    interface IElectric
    {
        int BatteryCapacity { get; }
    }

    interface IMechanical
    {
        string FuelType { get; }
    }

    interface IAutomatical
    {
    }

    interface IManual
    {
    }

    abstract class ACar : ICar
    {
        public string Brand { get; protected set; }
        public int Seats { get; protected set; }
        public string Multimedia { get; protected set; }

        protected ACar(string brand, int seats, string multimedia)
        {
            Brand = brand;
            Seats = seats;
            Multimedia = multimedia;
        }

        protected virtual string GetEngineDescription()
        {
            if (this is IElectric electricCar)
            {
                return $"электрокар с батареей {electricCar.BatteryCapacity} кВт·ч";
            }

            if (this is IMechanical mechanicalCar)
            {
                return $"обычный автомобиль на {mechanicalCar.FuelType}";
            }

            return "автомобиль";
        }

        protected virtual string GetTransmissionDescription()
        {
            if (this is IAutomatical)
                return "автоматической коробкой передач";

            if (this is IManual)
                return "механической коробкой передач";

            return "неизвестной коробкой передач";
        }

        public virtual string GetDescription()
        {
            return $"{Brand}: {GetEngineDescription()}, с {GetTransmissionDescription()}, {Seats} местами, {Multimedia} на борту";
        }
    }

    class TeslaCar : ACar, IElectric, IAutomatical
    {
        public int BatteryCapacity { get; private set; }

        public TeslaCar() : base("Tesla", 5, "Android Auto")
        {
            BatteryCapacity = 85;
        }
    }

    class BMWCar : ACar, IMechanical, IAutomatical
    {
        public string FuelType { get; private set; }

        public BMWCar() : base("BMW", 5, "iDrive")
        {
            FuelType = "бензине";
        }
    }

    class ToyotaCar : ACar, IMechanical, IManual
    {
        public string FuelType { get; private set; }

        public ToyotaCar() : base("Toyota", 5, "CarPlay")
        {
            FuelType = "бензине";
        }
    }

    class AudiCar : ACar, IMechanical, IAutomatical
    {
        public string FuelType { get; private set; }

        public AudiCar() : base("Audi", 5, "MMI")
        {
            FuelType = "дизеле";
        }
    }

    static class CarFactory
    {
        public static ICar Create(CarType carType)
        {
            switch (carType)
            {
                case CarType.Tesla:
                    return new TeslaCar();

                case CarType.BMW:
                    return new BMWCar();

                case CarType.Toyota:
                    return new ToyotaCar();

                case CarType.Audi:
                    return new AudiCar();

                default:
                    throw new ArgumentException("Неизвестный тип автомобиля");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Write("Введите марку автомобиля или done для остановки ввода: ");
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Пустой ввод. Попробуйте еще раз.");
                    continue;
                }

                if (input.Trim().ToLower() == "done")
                {
                    break;
                }

                if (Enum.TryParse(input, true, out CarType carType))
                {
                    ICar car = CarFactory.Create(carType);
                    Console.WriteLine(car.GetDescription());
                }
                else
                {
                    Console.WriteLine("Такая марка не поддерживается.");
                }
            }
        }
    }
}
