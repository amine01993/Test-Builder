using System.Data;
using AutoMapper;

using Test_Builder.Models;

namespace Test_Builder.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            PrimitiveTypesMapping();
            CustomerMapping();
            QuestionTypeMapping();
            CategoryMapping();
            PageMapping();
            TestMapping();
            TestQuestionMapping();
            AnswerMapping();
            QuestionMapping();
        }

        void PrimitiveTypesMapping()
        {
            CreateMap<DataRow, int?>().ForMember(d => d.Value, o => o.MapFrom(s => s["Nbr"]));
        }

        void CustomerMapping()
        {
            CreateMap<DataRow, Customer>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Table.Columns.Contains("Id") ? s["Id"] : 0))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Table.Columns.Contains("Name") ? s["Name"] : null))
                .ForMember(d => d.Password, o => o.MapFrom(s => s.Table.Columns.Contains("Password") ? s["Password"] : null))
                ;
        }

        void QuestionTypeMapping() {
            CreateMap<DataRow, QuestionType>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s["Id"]))
                .ForMember(d => d.Name, o => o.MapFrom(s => s["Name"]))
                .ForMember(d => d.Icon, o => o.MapFrom(s => s["Icon"]))
                .ForMember(d => d.Link, o => o.MapFrom(s => s["Link"]))
                ;
        }

        void CategoryMapping()
        {
            CreateMap<DataRow, Category>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s["Id"]))
                .ForMember(d => d.Name, o => o.MapFrom(s => s["Name"]))
                .ForMember(d => d.ParentId, o => o.MapFrom(s => s["ParentId"] == DBNull.Value ? null : s["ParentId"]))
                ;
        }

        void AnswerMapping()
        {
            var mappingExpression = CreateMap<DataRow, Answer>();

            mappingExpression.ForMember(d => d.Id, o => o.MapFrom(s => s["Id"]));
            mappingExpression.ForMember(d => d._Answer, o => o.MapFrom(s => s["_Answer"]));
            mappingExpression.ForMember(d => d.Correct, o => o.MapFrom(s => s["Correct"]));
            mappingExpression.ForMember(d => d.Match, o => o.MapFrom(s => s["Match"]));
            mappingExpression.ForMember(d => d.Points, o => o.MapFrom(s => s["Points"] == DBNull.Value ? null : s["Points"]));
            mappingExpression.ForMember(d => d.Penalty, o => o.MapFrom(s => s["Penalty"] == DBNull.Value ? null : s["Penalty"]));
            mappingExpression.ForMember(d => d.QuestionId, o => o.MapFrom(s => s.Table.Columns.Contains("QuestionId") ? s["QuestionId"] : 0));
        }

        void QuestionMapping()
        {
            CreateMap<DataRow, Question>()
                .ForMember(d => d.TypeId, o => o.MapFrom(s => s["TypeId"]))
                .ForMember(d => d.CategoryId, o => o.MapFrom(s => s["CategoryId"]))
                .ForMember(d => d.Points, o => o.MapFrom(s => s["Points"]))
                .ForMember(d => d.Penalty, o => o.MapFrom(s => s["Penalty"] == DBNull.Value ? null : s["Penalty"]))
                .ForMember(d => d.Shuffle, o => o.MapFrom(s => s["Shuffle"] == DBNull.Value ? null : s["Shuffle"]))
                .ForMember(d => d.Selection, o => o.MapFrom(s => s["Selection"] == DBNull.Value ? null : s["Selection"]))
                .ForMember(d => d._Question, o => o.MapFrom(s => s["_Question"]));
        }

        void TestMapping()
        {
            CreateMap<DataRow, Test>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s["Id"]))
                .ForMember(d => d.Name, o => o.MapFrom(s => s["Name"]))
                ;
        }

        void TestQuestionMapping()
        {
            CreateMap<DataRow, TestQuestion>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s["Id"]))
                .ForMember(d => d.Position, o => o.MapFrom(s => s["Position"]))
                .ForMember(d => d.Random, o => o.MapFrom(s => s["Random"]))

                .ForMember(d => d.QuestionId, o => o.MapFrom(s => s["QuestionId"] == DBNull.Value ? null : s["QuestionId"]))
                .ForMember(d => d.Question, o => o.MapFrom(s => s["Question"]))
                .ForMember(d => d.Selection, o => o.MapFrom(s => s["Selection"] == DBNull.Value ? null : s["Selection"]))

                .ForMember(d => d.QuestionIds, o => o.MapFrom(s => s["QuestionIds"] == DBNull.Value ? null : s["QuestionIds"]))
                .ForMember(d => d.Number, o => o.MapFrom(s => s["Number"] == DBNull.Value ? null : s["Number"]))

                .ForMember(d => d.TypeId, o => o.MapFrom(s => s["TypeId"] == DBNull.Value ? null : s["TypeId"]))
                .ForMember(d => d.TypeName, o => o.MapFrom(s => s["TypeName"]))

                //.ForMember(d => d.CategoryId, o => o.Ignore())
                //.ForMember(d => d.SubCategoryId, o => o.Ignore())
                //.ForMember(d => d.Answers, o => o.Ignore())
                ;
        }

        void PageMapping()
        {
            CreateMap<DataRow, Page>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s["Id"]))
                .ForMember(d => d.Name, o => o.MapFrom(s => s["Name"]))
                .ForMember(d => d.Limit, o => o.MapFrom(s => s["Limit"] == DBNull.Value ? null : s["Limit"]))
                .ForMember(d => d.Shuffle, o => o.MapFrom(s => s["Shuffle"]))
                .ForMember(d => d.Position, o => o.MapFrom(s => s["Position"]))
                .ForMember(d => d.TestId, o => o.MapFrom(s => s["TestId"]))
                ;
        }
    }
}