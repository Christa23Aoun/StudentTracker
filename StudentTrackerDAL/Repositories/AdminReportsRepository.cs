using Dapper;
using StudentTrackerCOMMON.DTOs.AdminReports;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerDAL.Infrastructure;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace StudentTrackerDAL.Repositories
{
    public class AdminReportsRepository : IAdminReportsRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public AdminReportsRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<ExcessiveAbsenceReportDto>> GetStudentsWithExcessiveAbsencesAsync(int absenceThreshold)
        {
            using IDbConnection connection = _connectionFactory.Create();

            var result = await connection.QueryAsync<ExcessiveAbsenceReportDto>(
                "dbo.sp_AdminReport_StudentsWithExcessiveAbsences",
                new { AbsenceThreshold = absenceThreshold },
                commandType: CommandType.StoredProcedure);

            return result.AsList();
        }

        public async Task<List<FailingStudentReportDto>> GetFailingStudentsAsync(decimal maxGrade)
        {
            using IDbConnection connection = _connectionFactory.Create();

            var result = await connection.QueryAsync<FailingStudentReportDto>(
                "dbo.sp_AdminReport_FailingStudents",
                new { MaxScore = maxGrade },
                commandType: CommandType.StoredProcedure);

            return result.AsList();
        }

        public async Task<List<ExcellentStudentReportDto>> GetExcellentStudentsAsync(decimal minGrade)
        {
            using IDbConnection connection = _connectionFactory.Create();

            var result = await connection.QueryAsync<ExcellentStudentReportDto>(
                "dbo.sp_AdminReport_ExcellentStudents",
                new { MinScore = minGrade },
                commandType: CommandType.StoredProcedure);

            return result.AsList();
        }
    }
}
