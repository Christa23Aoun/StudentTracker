using StudentTrackerCOMMON.Models;
using StudentTrackerDAL.Repositories;

public class TestService
{
    private readonly TestRepository _repository;

    public TestService(string connectionString)
    {
        _repository = new TestRepository(connectionString);
    }

    public Task<IEnumerable<Test>> GetAllAsync() => _repository.GetAllAsync();
    public Task<Test?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
    public Task<IEnumerable<Test>> GetByCourseIdAsync(int courseId) => _repository.GetByCourseIdAsync(courseId);

    // CREATE
    public async Task<int> CreateAsync(Test t)
    {
        if (string.IsNullOrWhiteSpace(t.TestName))
            throw new ArgumentException("Test name required.");

        if (t.Weight <= 0)
            throw new ArgumentException("Weight must be positive.");

        decimal total = await GetTotalWeightForCourseAsync(t.CourseID);

        if (total + t.Weight > 100)
            throw new ArgumentException("Total test weights cannot exceed 100%.");

        return await _repository.CreateAsync(t);
    }

    // UPDATE
    public async Task<int> UpdateAsync(Test t)
    {
        if (t.Weight <= 0)
            throw new ArgumentException("Weight must be positive.");

        decimal totalExisting = await GetTotalWeightForCourseAsync(t.CourseID);
        var oldTest = await _repository.GetByIdAsync(t.TestID);

        decimal totalWithoutOld = totalExisting - oldTest.Weight;

        if (totalWithoutOld + t.Weight > 100)
            throw new ArgumentException("Total test weights cannot exceed 100%.");

        return await _repository.UpdateAsync(t);
    }

    public Task<int> DeleteAsync(int id) => _repository.DeleteAsync(id);

    // TOTAL WEIGHT CALCULATION
    private async Task<decimal> GetTotalWeightForCourseAsync(int courseId)
    {
        var list = await _repository.GetByCourseIdAsync(courseId);
        return list.Sum(t => (decimal)t.Weight);
    }
}
