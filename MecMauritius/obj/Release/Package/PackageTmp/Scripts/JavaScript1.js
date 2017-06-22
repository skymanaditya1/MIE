

    // Method to fetch the resources when the query string is specified
    $(document).ready(function () {
        $("#btnSubmit").click(function () {
            $("#resource_head").empty();
            $.ajax({
                type: "POST",
                url: "@Url.Action('QuerySearch')",
                dataType: "json",
                data: {
                    queryName: $("#queryText").val()
                },
                success: function (data) {
                 
                    $.each(data, function (i, item) {

                        var resource_Url = item.ResourceURL;
                        
                        var append_data = $("<div class='col-md-3'>"
                        + "<div class='row'>"
                        + "<h3>" + item.ResourceTitle + "</h3>"
                        + "</div>"
                        + "<div class='row'>"
                        + "<ul class='img-list'>"
                        + "<li>"
                        + "<a href='" + item.ResourceURL + "' class='img-list'>"
                        + "<img src='" + item.ResourceThumbnail + "' alt='Resource' style='height:150px;width:100px;'/>"
                        + "<span class='text-content'><span>" + item.ResourceDescription + "</span></span>"
                        + "</a></li></ul>"
                        + "<div class='row'>"
                        + "<span class='label label-primary' style='margin:3px;font-size:small;'>"
                        + item.ResourceEducation
                        + "</span>"
                        + "<span class='label label-warning' style='margin:3px;font-size:small;'>"
                        + item.ResourceGrades
                        + "</span>"
                        + "<span class='label label-info' style='margin:3px;font-size:small;'>"
                        + item.ResourceSubject
                        + "</span>"
                       
                        + "<a href='" + item.ResourceURL + "' data-toggle='tooltip' title='Download Resource'>"
                        + "<img src=\"@Url.Content('~/Content/Resources/download_icon.png')\" alt='Download Resource' style='height:24px;width:24px;'/>" 
                        + "</a>"
                        + "</div>"
                        + "</div>");
                        
                        // var rows = "<tr>"
                          // + "<td>" + item.Name + "</td>"
                          // + "<td>" + item.Education + "</td>"
                          // + "<td>" + item.Grade + "</td>"
                          // + "<td>" + item.Subjects + "</td>"
                         // + "<td>" + "<img src= /fonts/" + iconType(item.Resource_Type) + ".png alt="Image">" + "</td>"
                          // + "<td><a href="" + item.URL + "">Download</a></td>"
                         // + "</tr>";

                        $("#resource_head").append(append_data);
                        
                    });
                },
                error: function (e) {
                    alert("Message : " + e.ToString());
                }
            });
            return false;
        })
    });

    var chosenGrade = "";
var chosenCourse = "";
var chosenResourceType = "";
    

var gradeUpload = "Select Grade";
var courseUpload = "Select Subject";
var resourceTypeUpload = "Select Resource Type";
var resourceDescription = "";
var resourceTitle = "";

$("#reset").click(function () {
    chosenGrade = chosenCourse = chosenResourceType = "";
    $("#GradeButton").html("Select Grade");
    $("#CourseButton").html("Select Course");
    $("#queryText").val("");
    fetch();
})

$("#drop").click(function () {
    $("#attributes").show();
})

//  $("#GradeUpload, #SubjectUpload, #ResourceUpload").change(validateForm);
$("#GradeUpload li").on("click", function () {

    $("#GUplBut").html($(this).text());
    $("#GForm").val($(this).text());
    gradeUpload = $(this).text();
    // validateForm();
});

$("#CourseUpload li").on("click", function () {

    $("#CUplBut").html($(this).text());
    $("#CForm").val($(this).text());
    courseUpload = $(this).text();
    //validateForm();
});

//$("#ResourceDescription").on("click", function () {
//    resourceDescription = $(this).val();
//    validateForm();
        
//})


//$("#ResourceTitle").on("click", function () {
//    resourceTitle = $(this).val();
//    validateForm();
//})

//$("#ResourceTypeUpload li").on("click", function () {

//    $("#RUplBut").html($(this).text());
//    $("#RForm").val($(this).text());
//    resourceTypeUpload = $(this).text();
//    validateForm();
//});

function validateForm() {



    if (gradeUpload == "Select Grade" || courseUpload == "Select Subject" || $("#ResourceTitle").val() == "" || $("#ResourceDescription").val() == "") {
        $("#drop").hide();
        $("#success").hide();
        $("#warning").show();
    }
    else {
        $("#warning").hide();
        $("#success").show();
        $("#drop").show();
    }

}

$("#GradeList li").on("click", function () {
    $("#GradeButton").html($(this).text());
    chosenGrade = $(this).text();
    fetch();
});

$("#CourseList li").on("click", function () {
    $("#CourseButton").html($(this).text());
    chosenCourse = $(this).text();
    fetch();
});

$("#ResourceTypeList li").on("click", function () {

    chosenResourceType = $(this).text().valueOf() == new String("All").valueOf() ? "" : $(this).text();
    fetch();
});


//$("#file").change(function () {
//    var x = this;
//    var txt = "";

//    if ("files" in x) {
//        if (x.files.length == 0) {
//            window.alert("Select one or more files.");
//        } else {
//            $("#tblResources").css("display", "hidden");
//            $("#uploadTbl").css("display", "inline");
//            $("#attributes").css("display", "inline")
//            $("#uploadTbl tbody").remove();
//            $("#uploadTbl").append("<tbody> </tbody>");

//            for (var i = 0; i < x.files.length; i++) {
//                txt += "<tr>" + "<td>" + (i + 1) + "</td>";
//                var file = x.files[i];
//                if ("name" in file) {
//                    var fname = file.name;

//                    txt += "<td class="uploadTd">" + fname.substring(0, fname.indexOf(".")) + "</td>";
//                    txt += "<td style="max-width: 10px;" >" + fname.substring(fname.indexOf(".")) + "</td>";
//                }
//                if ("size" in file) {
//                    txt += "<td>" + file.size + "</td></tr>";

//                }
//            }
//        }

//    }
//    $("#uploadTbl tbody").append(txt);

//})





$(document).ready(function () {
    $("#GradeList, #CourseList, #ResourceType").change(fetch);
    $("#drop, #uploadTbl").hide();



});



function iconType(resource) {
    switch (resource) {
        case "E-Books": return "E-Books"; break;
        case "Image": return "Image"; break;
        case "Compressed": return "Compressed"; break;
        case "Video": return "Video"; break;
        case "HTML5": return "HTML5"; break;
        default: return "File";
            break;

    }

}

function fetch() {
    // $("#tblResources").show();
    //$("#tblResources tbody tr").remove();
    $("#resource_head").empty();
    // $("#gridview").empty();
    $.ajax({
        type: "POST",
        url: "@Url.Action('GetResourcesDropDowns')",
        dataType: "json",
        data: {
            grade: chosenGrade, subject: chosenCourse,
            type: chosenResourceType
        },
        success: function (data) {
            // var items = "";
            $.each(data, function (i, item) {

                var resource_Url = item.ResourceURL;
                // var resource_url = item.ResourceURL;
                var append_data = $("<div class='col-md-4'>"
                + "<div class='row'>"
                + "<h3>" + item.ResourceTitle + "</h3>"
                + "</div>"
                + "<div class='row'>"
                + "<ul class='img-list'>"
                + "<li>"
                + "<a href='" + item.ResourceURL + "' class='img-list'>"
                + "<img src='" + item.ResourceThumbnail + "' alt='Resource' style='height:150px;width:150px;'/>"
                + "<span class='text-content'><span>" + item.ResourceDescription + "</span></span>"
                + "</a></li></ul>"
                + "<div class='row'>"
                + "<span class='label label-primary' style='margin:3px;font-size:small;'>"
                + item.ResourceEducation
                + "</span>"
                + "<span class='label label-warning' style='margin:3px;font-size:small;'>"
                + item.ResourceGrades
                + "</span>"
                + "<span class='label label-info' style='margin:3px;font-size:small;'>"
                + item.ResourceSubject
                + "</span>"

                + "<a href='" + item.ResourceURL + "' data-toggle='tooltip' title='Download Resource'>"
                + "<img src=\"@Url.Content('~/Content/Resources/download_icon.png')\" alt='Download Resource' style='height:24px;width:24px;'/>"
                + "</a>"
                + "</div>"
                + "</div>");

                /**var rows = "<tr>"
                    + "<td>" + item.Name + "</td>"
                    + "<td>" + item.Education + "</td>"
                    + "<td>" + item.Grade + "</td>"
                    + "<td>" + item.Subjects + "</td>"
                    + "<td>" + "<img src= /fonts/" + iconType(item.Resource_Type) + ".png alt="Image">" + "</td>"
                    + "<td><a href="" + item.URL + "">Download</a></td>"
                    + "</tr>";*/

                $("#resource_head").append(append_data);

            });
        },
        error: function (e) {
            alert("Message : " + e.ToString());
        }
    });
    return false;
}
