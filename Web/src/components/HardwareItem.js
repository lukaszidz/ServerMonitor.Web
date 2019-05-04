import React from 'react';
import PropTypes, { number } from 'prop-types';
import { Row, Col } from 'antd';
import Chart from './Chart/chart';

const HardwareItem = ({ item }) => {
  const keys = Object.keys(item);
  const childs = keys
    .map(key =>
     <Col key={key} span={8}>
      <Chart title={key} values={item[key]} />
    </Col>);
  return (
    <Row>
      {childs}
    </Row>
  );
};


HardwareItem.propTypes = {
  item: PropTypes.shape({
    key: PropTypes.string.isRequired,
    data: PropTypes.arrayOf(PropTypes.shape({
      key: PropTypes.string,
      array: PropTypes.arrayOf(number),
      Value: PropTypes.string,
    }).isRequired)
  }).isRequired
};

export default HardwareItem;
