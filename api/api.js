const range = len => {
  const arr = [];
  for (let i = 0; i < len; i++) {
    arr.push(i);
  }
  return arr;
};
const newPerson = () => {
  const statusChance = Math.random();
  return {
    firstName: "asd",
    lastName: "zxcasf",
    age: Math.floor(Math.random() * 30),
    visits: Math.floor(Math.random() * 100),
    status: statusChance > 0.66 ?
      "relationship" :
      statusChance > 0.33 ? "complicated" : "single"
  };
};

export function makeData(len = 10) {
  return range(len).map(d => {
    return {
      key: d,
      ...newPerson()
    };
  });
}