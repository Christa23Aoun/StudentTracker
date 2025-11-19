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

    public async Task<IEnumerable<Test>> GetByCourseIdAsync(int courseId)
        => await _repository.GetByCourseIdAsync(courseId);

    public async Task<int> CreateAsync(Test t)
    {
        if (string.IsNullOrWhiteSpace(t.TestName))
            throw new ArgumentException("Test name required.");

        if (t.Weight <= 0 || t.MaxScore <= 0)
            throw new ArgumentException("Weight and MaxScore must be positive.");

        return await _repository.CreateAsync(t);
    }

    public Task<int> UpdateAsync(Test t) => _repository.UpdateAsync(t);
    public Task<int> DeleteAsync(int id) => _repository.DeleteAsync(id);
}
