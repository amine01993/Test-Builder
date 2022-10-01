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
            PageQuestionMapping();
            AnswerMapping();
            QuestionMapping();
        }

        void PrimitiveTypesMapping()
        {
            CreateMap<DataRow, PrimitiveType<int>>().ForMember(d => d.Value, o => o.MapFrom(s => s[s.Table.Columns[0].ColumnName]));
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
                .ForMember(d => d.Id, o => o.MapFrom(s => MapProperty(s, "QuestionType", "Id"))) // s[Id]
                .ForMember(d => d.Name, o => o.MapFrom(s => MapProperty(s, "QuestionType", "Name")))
                .ForMember(d => d.Icon, o => o.MapFrom(s => s.Table.Columns.Contains("Icon") ? s["Icon"] : null))
                .ForMember(d => d.Link, o => o.MapFrom(s => s.Table.Columns.Contains("Link") ? s["Link"] : null))
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
                .ForMember(d => d.Id, o => o.MapFrom(s => MapProperty(s, "Question", "Id")))
                .ForMember(d => d.TypeId, o => o.MapFrom(s => MapProperty(s, "Question", "TypeId")))
                .ForMember(d => d.QuestionType, o => o.MapFrom(s => s))
                .ForMember(d => d.CategoryId, o => o.MapFrom(s => MapProperty(s, "Question", "CategoryId")))
                .ForMember(d => d.Points, o => o.MapFrom(s => MapProperty(s, "Question", "Points")))
                .ForMember(d => d.Penalty, o => o.MapFrom(s => MapProperty(s, "Question", "Penalty")))
                .ForMember(d => d.Shuffle, o => o.MapFrom(s => MapProperty(s, "Question", "Shuffle")))
                .ForMember(d => d.Selection, o => o.MapFrom(s => MapProperty(s, "Question", "Selection")))
                .ForMember(d => d._Question, o => o.MapFrom(s => MapProperty(s, "Question", "_Question")))
                ;
        }

        void TestMapping()
        {
            CreateMap<DataRow, Test>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s["Id"]))
                .ForMember(d => d.Name, o => o.MapFrom(s => s["Name"]))
                ;
        }

        void PageQuestionMapping()
        {
            CreateMap<DataRow, PageQuestion>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s["Id"]))
                .ForMember(d => d.Position, o => o.MapFrom(s => s["Position"]))
                .ForMember(d => d.Random, o => o.MapFrom(s => s["Random"]))

                .ForMember(d => d.QuestionId, o => o.MapFrom(s => s["QuestionId"] == DBNull.Value ? null : s["QuestionId"]))
                .ForMember(d => d.Question, o => o.MapFrom(s => s))

                .ForMember(d => d.QuestionIds, o => o.MapFrom(s => s["QuestionIds"] == DBNull.Value ? null : s["QuestionIds"]))
                .ForMember(d => d.Number, o => o.MapFrom(s => s["Number"] == DBNull.Value ? null : s["Number"]))


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

        private object MapProperty(DataRow dr, string mapping, string property)
        {
            var name = dr.Table.TableName;
            string propertyFullName = property;
            if(mapping == "QuestionType")
            {
                if(name == "Question")
                {
                    propertyFullName = mapping + "." + property;
                }
                else if(name == "PageQuestion")
                {
                    propertyFullName = "Question." + mapping + "." + property;
                }
            }
            else if(mapping == "Question")
            {
                if (name == "PageQuestion")
                {
                    propertyFullName = mapping + "." + property;
                }
            }
            if (dr.Table.Columns.Contains(propertyFullName) && dr[propertyFullName] != DBNull.Value)
                return dr[propertyFullName];
            return null;
        }
    }
}