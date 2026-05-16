const tasks=[
 {label:"Важно / Не срочно",items:[["Задача абобная","2 часа","2 часа"],["Задача абобная","2 часа","2 часа"]]},
 {label:"Важно / Срочно",items:[["Задача абобная","2 часа","2 часа"],["Задача абобная","2 часа","2 часа"]]},
 {label:"Не важно / Не срочно",items:[["Задача абобная","2 часа","2 часа"],["Задача абобная","2 часа","2 часа"]]},
 {label:"Не важно / Срочно",items:[["Задача абобная","2 часа","2 часа"],["Задача абобная","2 часа","2 часа"]]}
];

const matrix=document.querySelector("#taskMatrix");

function row([title,complexity,deadline]){
  const el=document.createElement("article");
  el.className="task-row";
  el.innerHTML=`
    <div class="task-title">${title}</div>
    <div class="divider"></div>
    <div class="task-meta"><span>сложность</span><span>${complexity}</span></div>
    <div class="divider"></div>
    <div class="task-meta"><span>Дедлайн через</span><span>${deadline}</span></div>
  `;
  return el;
}

tasks.forEach(group=>{
  const q=document.createElement("section");
  q.className="quadrant";
  q.dataset.label=group.label;
  group.items.forEach(item=>q.appendChild(row(item)));
  matrix.appendChild(q);
});
