import React from 'react';
import PropTypes from 'prop-types';
import { Card, Tag } from 'antd';
import filesize from 'filesize';
import {
  AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip,
} from 'recharts';

const Title = ({ title, text }) => (
  <div>
    {title}
    {text && <Tag>{filesize(text)}</Tag>}
  </div>
);
Title.defaultProps = { text: '' };

Title.propTypes = {
  title: PropTypes.string.isRequired,
  text: PropTypes.string
};

const Chart = ({ title, values, text }) => (
  <Card className="chart-card" title={(<Title text={text} title={title} />)}>
    <AreaChart
        width={490}
        height={400}
        data={values.map(function(v, i) { return { name: 5 * i, [title]: v.value} })}
        margin={{
          top: 10, right: 30, left: 0, bottom: 0,
        }}
      >
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="name" />
        <YAxis domain={[0, 100]} />
        <Tooltip />
        <Area type="monotone" dataKey={title} stroke="#8884d8" fill="#8884d8" />
      </AreaChart>
  </Card>
);
Chart.defaultProps = { text: '' };

Chart.propTypes = {
  title: PropTypes.string.isRequired,
  value: PropTypes.array.isRequired,
  text: PropTypes.string
};
export default Chart;
