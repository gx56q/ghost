using System;
using System.Text;

namespace hashes;

public class GhostsTask : 
	IFactory<Document>, IFactory<Vector>, IFactory<Segment>, IFactory<Cat>, IFactory<Robot>, 
	IMagic
{
	private byte[] documentArray = { 1, 1, 2 };
	Document document;
	Segment segment;
	Cat cat = new Cat("Murka", "Murka", DateTime.Today);
	Vector vector = new Vector(1, 1);
	Robot robot = new Robot("R2D2");
	
	public GhostsTask()
	{
		document = new Document("Ghosts", Encoding.UTF8, documentArray);
		segment = new Segment(vector, vector);
	}
	public void DoMagic()
	{
		documentArray[0] = 10;
		vector.Add(new Vector(2, 28));
		cat.Rename("Bonifacy");
		Robot.BatteryCapacity++;
	}

	// Чтобы класс одновременно реализовывал интерфейсы IFactory<A> и IFactory<B> 
	// придется воспользоваться так называемой явной реализацией интерфейса.
	// Чтобы отличать методы создания A и B у каждого метода Create нужно явно указать, к какому интерфейсу он относится.
	// На самом деле такое вы уже видели, когда реализовывали IEnumerable<T>.

	Vector IFactory<Vector>.Create()
	{
		return vector;
	}

	Segment IFactory<Segment>.Create()
	{
		return segment;
	}

	Document IFactory<Document>.Create()
	{
		return document;
	}

	Cat IFactory<Cat>.Create()
	{
		return cat;
	}

	Robot IFactory<Robot>.Create()
	{
		return robot;
	}
}