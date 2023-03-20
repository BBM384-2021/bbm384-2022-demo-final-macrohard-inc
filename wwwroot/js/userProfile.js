let isListed = false;

function listUsers() {
    if (!isListed) {
        $.ajax({
            type: "GET",
            url: "/Profile/ListUsers",
            dataType: 'json',
            success: function (response) {
                let tableBody = $("table tbody")
                for (let i = 0; i < response.length; i++) {
                    const firstName = response[i].firstName;
                    const lastName = response[i].lastName;
                    const id = response[i].id;
                    let str = "<tr><td>" + firstName + "</td><td>" + lastName + "</td><td>" +
                        '<a href="/Profile/ViewProfile?id=' + id + '"' + '> View </a>';
                    tableBody.append(str);
                    isListed = true;
                }
            },
            error: function () {
                alert("An error occured");
            }
        })
    }
}

function filter() {
    let input, filter, table, tr, td1, td2, i, txtValue1, txtValue2;
    input = document.getElementById("myInput");
    filter = input.value.toUpperCase();
    table = document.getElementById("grid-basic");
    tr = table.getElementsByTagName("tr");
    for (i = 0; i < tr.length; i++) {
        td1 = tr[i].getElementsByTagName("td")[0];
        td2 = tr[i].getElementsByTagName("td")[1];
        if (td1 && td2) {
            txtValue1 = td1.textContent;
            txtValue2 = td2.textContent;
            if (txtValue1.toUpperCase().indexOf(filter) > -1 || txtValue2.toUpperCase().indexOf(filter) > -1) {
                tr[i].style.display = "";
            } else {
                tr[i].style.display = "none";
            }
        }
    }
    if (input.value.length !== 0) {
        $("#grid-basic").show();
    } else {
        console.log("here");
        document.getElementById("grid-basic").style.display = "none";
    }
}

function popup(tagName,id,container) {
    var modalCont = document.getElementById(container);
    var modal = document.getElementById(id);
    modalCont.classList.toggle(tagName.value)
    modal.classList.toggle(tagName.value)
};

function popup2(id,container) {
    var modalCont = document.getElementById(container);
    var modal = document.getElementById(id);
    modalCont.classList.remove("hide");
    modal.classList.remove("hide");
};

/*function popup3(id) {
    var modalCont2 = document.getElementById("modalContainer2");
    var modal = document.getElementById("settingsModalContainer");
    modalCont2.classList.remove("hide");
    modal.classList.remove("hide");
};
function popup4() {

    var modalCont2 = document.getElementById("modalContainer2");
    var modal = document.getElementById("settingsModalContainer");
    modalCont2.classList.add("hide");
    modal.classList.add("hide");
}*/
